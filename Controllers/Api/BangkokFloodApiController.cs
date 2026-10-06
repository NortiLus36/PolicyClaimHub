using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PolicyClaimHub.Dtos.BangkokFlood;

namespace PolicyClaimHub.Controllers.Api;

[ApiController]
[Route("api/bangkok-flood")]
public sealed class BangkokFloodApiController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BangkokFloodApiController> _logger;

    public BangkokFloodApiController(
        IHttpClientFactory httpClientFactory,
        ILogger<BangkokFloodApiController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet("road-points")]
    [ProducesResponseType<BangkokRoadFloodResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<BangkokRoadFloodResponseDto>> GetRoadPoints(
        CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("NowBangkok");
            var data = await client.GetFromJsonAsync<BangkokRoadFloodResponseDto>(
                "road-flood-data.json",
                cancellationToken);

            if (data is null)
            {
                return Problem(
                    title: "NOW Bangkok ไม่ส่งข้อมูลกลับมา",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }

            return Ok(data);
        }
        catch (Exception exception) when (
            exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(
                exception,
                "Unable to load Bangkok road flood data from NOW Bangkok.");

            return Problem(
                title: "ไม่สามารถเชื่อมต่อข้อมูลน้ำท่วมถนนของ กทม. ได้",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
