using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vessel.Application.DTOs.Bookings;
using Vessel.Application.Interfaces.Bookings;
using Vessel.Core.Enums;

namespace Vessel.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Tags("Bookings")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Creates a new booking.
    /// </summary>
    /// <param name="dto">The booking details.</param>
    /// <param name="idempotencyKey">Client-side unique key to prevent duplicate bookings.</param>
    /// <returns>The created booking.</returns>
    [HttpPost]
    [Authorize(Policy = "ConsumerOnly")]
    [ProducesResponseType(typeof(BookingResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<BookingResponseDto>> Create(
        [FromBody] CreateBookingDto dto,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return BadRequest("Idempotency-Key header is required.");
        }

        var userId = GetUserId();
        try
        {
            var booking = await _bookingService.CreateBookingAsync(userId, dto, idempotencyKey);
            return CreatedAtAction(nameof(GetMyBookings), new { }, booking);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Gets all bookings for the authenticated consumer.
    /// </summary>
    /// <returns>A list of bookings.</returns>
    [HttpGet("my-bookings")]
    [Authorize(Policy = "ConsumerOnly")]
    [ProducesResponseType(typeof(IEnumerable<BookingResponseDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetMyBookings()
    {
        var userId = GetUserId();
        var bookings = await _bookingService.GetConsumerBookingsAsync(userId);
        return Ok(bookings);
    }

    /// <summary>
    /// Gets all booking requests for the authenticated provider.
    /// </summary>
    /// <returns>A list of booking requests.</returns>
    [HttpGet("requests")]
    [Authorize(Policy = "ProviderOnly")]
    [ProducesResponseType(typeof(IEnumerable<BookingResponseDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetRequests()
    {
        var userId = GetUserId();
        var bookings = await _bookingService.GetProviderRequestsAsync(userId);
        return Ok(bookings);
    }

    /// <summary>
    /// Updates the status of a booking.
    /// </summary>
    /// <param name="id">The booking ID.</param>
    /// <param name="dto">The new status.</param>
    /// <returns>The updated booking.</returns>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "ProviderOnly")]
    [ProducesResponseType(typeof(BookingResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<BookingResponseDto>> UpdateStatus(Guid id, [FromBody] UpdateBookingStatusDto dto)
    {
        var providerId = GetUserId();
        try
        {
            var booking = await _bookingService.UpdateStatusAsync(providerId, id, dto.Status);
            return Ok(booking);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("User not found in token.");
        }
        return userId;
    }
}
