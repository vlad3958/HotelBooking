namespace HotelBooking.Infrastructure.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using HotelBooking.Domain;
using HotelBooking.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

public class AdminRepository : IAdmin, IHotel, IRoom
{
    private readonly HotelBookingDbContext _dbContext;

    public AdminRepository(HotelBookingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IEnumerable<Booking> GetAllBookings()
    {
        return _dbContext.Bookings
            .Include(b => b.Room)
            .ThenInclude(r => r.Hotel)
            .ToList();
    }
    public IEnumerable<Booking> GetBookingsOverlapping(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate) throw new ArgumentException("End date must be after start date");
        return _dbContext.Bookings
            .Where(b => b.StartDate < endDate && b.EndDate > startDate)
            .AsNoTracking()
            .ToList();
    }
    public BookingStatsData GetBookingStatsData(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate) throw new ArgumentException("End date must be after start date");
        var bookings = _dbContext.Bookings
            .Where(b => b.StartDate < endDate && b.EndDate > startDate)
            .AsNoTracking()
            .ToList();
        int total = bookings.Count;
        int distinctRooms = bookings.Select(b => b.RoomId).Distinct().Count();
        int distinctUsers = bookings.Select(b => b.UserId).Where(u => !string.IsNullOrEmpty(u)).Distinct().Count();
        int roomNights = 0;
        foreach (var b in bookings)
        {
            var effStart = b.StartDate < startDate ? startDate : b.StartDate;
            var effEnd = b.EndDate > endDate ? endDate : b.EndDate;
            var nights = (effEnd.Date - effStart.Date).Days;
            if (nights > 0) roomNights += nights;
        }
        return new BookingStatsData
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalBookings = total,
            DistinctRooms = distinctRooms,
            DistinctUsers = distinctUsers,
            TotalRoomNights = roomNights
        };
    }
    public Hotel AddHotel(string name, string address, string description)
    {
        var hotel = new Hotel
        {
            Name = name,
            Address = address,
            Description = description
        };
        _dbContext.Hotels.Add(hotel);
        _dbContext.SaveChanges();
        return hotel;
    }

    public Hotel UpdateHotel(int id, string name, string address, string description)
    {
        var existingHotel = _dbContext.Hotels.Find(id);
        if (existingHotel == null)
        {
            throw new Exception("Hotel not found");
        }

        existingHotel.Name = name;
        existingHotel.Address = address;
        existingHotel.Description = description;

        _dbContext.SaveChanges();
        return existingHotel;
    }
    public void RemoveHotel(int hotelId)
    {
        var hotel = _dbContext.Hotels.Find(hotelId);
        if (hotel == null)
        {
            throw new Exception("Hotel not found");
        }

        _dbContext.Hotels.Remove(hotel);
        _dbContext.SaveChanges();
    }

    public void AddRoom(int hotelId, string name, double price, int capacity, DateTime? startDate, DateTime? endDate)
    {
        var room = new Room
        {
            HotelId = hotelId,
            Name = name,
            Price = price,
            Capacity = capacity,
            StartDate = startDate,
            EndDate = endDate
        };
        _dbContext.Rooms.Add(room);
        _dbContext.SaveChanges();
    }
    public void UpdateRoom(int id, string name, double price, int capacity)
    {
        var existingRoom = _dbContext.Rooms.Find(id);
        if (existingRoom == null)
        {
            throw new Exception("Room not found");
        }
    
        existingRoom.Price = price;
        existingRoom.Capacity = capacity;
        existingRoom.Name = name;
    
        _dbContext.SaveChanges();
    }
    public void RemoveRoom(int roomId)
    {
        var room = _dbContext.Rooms.Find(roomId);
        if (room == null)
        {
            throw new Exception("Room not found");
        }

        _dbContext.Rooms.Remove(room);
        _dbContext.SaveChanges();
    }
}