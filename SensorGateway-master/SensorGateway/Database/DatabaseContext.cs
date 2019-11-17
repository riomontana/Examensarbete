using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SensorGateway.Models;
using SensorGateway.Services;

namespace SensorGateway.Database
{
    public class DatabaseContext : DbContext, IDataStoreSensor<Sensor>
    { 

        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<DateAndInterval> DateAndIntervals { get; set; }

        public DatabaseContext(string databasePath)
        {
            DatabasePath = databasePath;
        }

        public static DatabaseContext Create(string databasePath)
        {
            var dbContext = new DatabaseContext(databasePath);
            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();
            return dbContext;
        }

        private string DatabasePath { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={DatabasePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // make ID property primary key
            modelBuilder.Entity<Sensor>()
                .HasKey(p => p.Id);

            // require Name to be set
            modelBuilder.Entity<Sensor>()
                .Property(p => p.Name)
                .IsRequired();

            // require Uuid to be set
            modelBuilder.Entity<Sensor>()
                .Property(p => p.Uuid)
                .IsRequired();

            // require IsSelected to be set
            modelBuilder.Entity<Sensor>()
                .Property(p => p.IsActive)
                .IsRequired();

            // make ID property primary key
            modelBuilder.Entity<Sensor>()
                .HasKey(p => p.Id);

            // require Name to be set
            modelBuilder.Entity<Sensor>()
                .Property(p => p.Name)
                .IsRequired();

            // require IsActive to be set
            modelBuilder.Entity<Sensor>()
                .Property(p => p.IsActive)
                .IsRequired();
                
        }

        public async Task<Sensor> GetSensorAsync(int id)
        {
            try
            {
                var sensor = await Sensors.FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);
                return sensor;
            }
            catch (Exception e)
            {
                Console.WriteLine("Get Sensor error: " + e);
                return null;
            }
        }

        public async Task<IEnumerable<Sensor>> GetSensorsAsync(bool forceRefresh = false)
        {
            try
            {
                var allSensors = await Sensors.Include(x => x.DateAndInterval).ToListAsync().ConfigureAwait(false);

                return allSensors;
            }
            catch (Exception e)
            {
                Console.WriteLine("Get Sensors error: " + e);
                return null;
            }
        }

        // Todo add error handling
        public async Task<bool> AddSensorAsync(Sensor sensor)
        {
            try
            {
                Sensors.Add(sensor);
                await SaveChangesAsync().ConfigureAwait(false);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Add Sensor error: " + e);
                return false;
            }
        }

        // Todo add error handling
        public async Task<bool> UpdateSensorAsync(Sensor sensor)
        {
            try
            {
                DateAndIntervals.Update(sensor.DateAndInterval);
                await SaveChangesAsync().ConfigureAwait(false);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Update Sensor error: " + e);
                return false;
            }
        }

        // Todo add error handling
        public async Task<bool> DeleteSensorAsync(int id)
        {
            try
            {
                var sensorToRemove = Sensors.FirstOrDefault(x => x.Id == id);

                if (sensorToRemove != null)
                {
                    Sensors.Remove(sensorToRemove);

                    DateAndIntervals.Remove(sensorToRemove.DateAndInterval);

                    await SaveChangesAsync().ConfigureAwait(false);
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Remove Sensor error: " + e);
                return false;
            }
        }
    }
}
