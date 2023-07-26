using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShortLinkGen.BusinessLogic;

namespace ShortLinkGen.Controllers
{
    [EnableCors("AllowAll")]
    [Route("api")]
    [ApiController]
    public class GenController : ControllerBase
    {
        private readonly ShortLinkProcessor _processor;

        public GenController(ShortLinkProcessor processor)
        {
            _processor = processor;
        }

        /// <summary>
        /// Метод действия получения сущности
        /// </summary>
        /// <param name="componentType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("link/getshort")]
        public async Task<IActionResult> GenerateShortLink([FromBody] ModelOperation model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.LongUrl))
                    throw new Exception("Не передан обязательный параметр Url для работы метода генерации короткой ссылки");

                bool resultCheckUri = Uri.TryCreate(model.LongUrl, UriKind.RelativeOrAbsolute, out var uriSource)
                    && (uriSource.Scheme == Uri.UriSchemeHttp || uriSource.Scheme == Uri.UriSchemeHttps);

                if (resultCheckUri == false)
                    throw new Exception("Не удалось привести переданную строку к типу Uri. Передайте корректный адрес страницы");

                var result = await _processor.Create(new ShortUri { Source = model.LongUrl });

                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
