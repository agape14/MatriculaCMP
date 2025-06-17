using MatriculaCMP.Services;
using Microsoft.AspNetCore.Mvc;
using static MatriculaCMP.Shared.FirmaDigitalDTO;

namespace MatriculaCMP.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class FirmaDigitalController : ControllerBase
    {
        private readonly FirmaDigitalService _firmaService;

        public FirmaDigitalController(FirmaDigitalService firmaService)
        {
            _firmaService = firmaService;
        }

        [HttpPost("firmar")]
        public async Task<ActionResult<UploadResponse>> FirmarDocumento([FromBody] FirmaRequest request)
        {
            var result = await _firmaService.FirmarDocumentoAsync(request);
            return Ok(result);
        }

        [HttpPost("upload")]
        public async Task<ActionResult<DownloadResponse>> SubirDocumentoFirmado([FromBody] UploadRequest request)
        {
            var result = await _firmaService.SubirDocumentoFirmadoAsync(request);
            return Ok(result);
        }
    }
}
