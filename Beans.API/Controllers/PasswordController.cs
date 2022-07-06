using Beans.API.Models;
using Beans.Common;
using Beans.Common.Attributes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Beans.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/[controller]")]
public class PasswordController : ControllerBase
{
    private readonly List<string> _words = new();
    private readonly int _count;
    private readonly Random _random = new();
    private readonly List<int> _badIntegers = new();

    public PasswordController(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var filename = Path.Join(environment.WebRootPath, Constants.WordListFilename);
        if (string.IsNullOrWhiteSpace(filename))
        {
            throw new InvalidOperationException("Word list name is empty");
        }
        var json = System.IO.File.ReadAllText(filename);
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException("No words in word list");
        }
        var words = JsonConvert.DeserializeObject<string[]>(json)?.ToList() ?? new();
        if (words is null || !words.Any())
        {
            throw new InvalidOperationException("No words in word list");
        }
        var minLength = 0;
        var mls = configuration["Generate:MinLength"];
        if (!string.IsNullOrWhiteSpace(mls))
        {
            _ = int.TryParse(mls, out minLength);
        }
        if (minLength > 0)
        {
            var removedwords = words.Where(x => x.Length < minLength).ToList();
            if (removedwords is not null && removedwords.Any())
            {
                foreach (var word in removedwords)
                {
                    words.Remove(word);
                }
            }
        }
        _words = words;
        _count = _words.Count;
        //
        // some numbers are not to be used
        //
        var bisection = configuration.GetSection("ProscribedNumbers");
        if (bisection is not null)
        {
            var ints = bisection.Get<int[]>();
            if (ints is not null && ints.Any())
            {
                _badIntegers.AddRange(ints);
            }
        }
    }

    [HttpGet]
    [Route("Word")]
    public IActionResult Word() => Ok(JsonConvert.SerializeObject(_words[_random.Next(_count)]));

    [HttpGet]
    [Route("Words/{count}")]
    public IActionResult Words(int count)
    {
        if (count < 0)
        {
            return BadRequest(new ApiError(string.Format(Strings.Invalid, "count")));
        }
        if (count == 0)
        {
            return Ok(Array.Empty<string>());
        }
        var ret = new string[count];
        for (var i = 0; i < count; i++)
        {
            ret[i] = _words[_random.Next(_count)];
        }
        return Ok(ret);
    }

    [HttpGet]
    [Route("Generate/{words}/{digits}")]
    public IActionResult Generate(int words, int digits)
    {
        if (words <= 0 || digits <= 0)
        {
            return BadRequest(string.Format(Strings.BadGenerate, words, digits));
        }
        if (words == 1)
        {
            return Word();
        }
        var w = new string[words];
        var d = new string[words - 1];
        for (var i = 0; i < words; i++)
        {
            w[i] = _words[_random.Next(_count)];
        }
        for (var i = 0; i < words - 1; i++)
        {
            var num = _random.Next(8999) + 1000;
            while (_badIntegers.Contains(num))
            {
                num = _random.Next(8999) + 1000;
            }
            d[i] = num.ToString("d4");
        }
        return Ok(Interleave(w, d));
    }

    private static string Interleave(string[] outside, string[] inside)
    {
        if (inside.Length != outside.Length - 1)
        {
            return string.Empty;
        }
        var sb = new StringBuilder();
        for (var i = 0; i < outside.Length; i++)
        {
            sb.Append(outside[i]);
            if (i < inside.Length)
            {
                sb.Append(inside[i]);
            }
        }
        return sb.ToString();
    }
}
