﻿using Microsoft.AspNetCore.Mvc;
using SonosSQLiteWrapper.Interfaces;

namespace Sonos.Controllers
{
    [Route("/[controller]")]
    public class TestController : Controller
    {
        public TestController()
        {
        }
        [HttpGet("test")]
        public string Test()
        {
            throw new System.Exception("dies ist ein blub");
            //return "huuuuu";
        }
    }
}
