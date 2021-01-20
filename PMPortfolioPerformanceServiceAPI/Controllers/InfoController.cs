using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PMPortfolioPerformanceServiceAPI.Models;

namespace PMPortfolioPerformanceServiceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        private readonly IOptions<ServiceInfo> _config;
        public InfoController(IOptions<ServiceInfo> appConfig)
        {
            _config = appConfig;
        }

        // GET: api/Info
        [HttpGet]
        public string Info()
        {
            return $"{_config.Value.ServiceName}\nVersion :{_config.Value.Version}";
        }

        [HttpGet]
        [Route("InfoJson")]
        public ServiceInfo InfoJson()
        {
            ServiceInfo info = new ServiceInfo()
            {
                ServiceName = _config.Value.ServiceName,
                Version = _config.Value.Version
            };

            return info;
        }
    }
}
