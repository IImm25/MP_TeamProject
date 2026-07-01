using AutoMapper;
using Backend.Data.DTO.Location;
using Backend.Data.Entitites;
using Backend.Data.Repositories;

namespace Backend.Web.Services
{
    public class LocationService
    {
        private readonly IRepository<Location> locations;
        private readonly IMapper mapper;

        public LocationService(IRepository<Location> locations, IMapper mapper)
        {
            this.locations = locations;
            this.mapper = mapper;
        }

        public async Task<LocationResponseDto> CreateLocation(LocationCreateDto createInfo)
        {
            Location tur = new Location(createInfo.Name,createInfo.Latitude,createInfo.Longitude, false);
            int id = await locations.AddAsync(tur);
            return mapper.Map<LocationResponseDto>(await locations.GetByIdAsync(id));
        }

        public async Task<List<LocationResponseDto>> GetAll()
        {
            return mapper.Map<List<LocationResponseDto>>(await locations.GetAllAsync());
        }

        public async Task<LocationResponseDto?> GetLocation(int id)
        {
            return mapper.Map<LocationResponseDto?>(await locations.GetByIdAsync(id));
        }

        public async Task<LocationResponseDto?> UpdateLocation(int id, LocationUpdateDto update)
        {
            var turbine = await locations.GetByIdAsync(id);
            if (turbine == null)
            {
                return null;
            }
            if (update.Name != null) turbine.Name = update.Name;
            if (update.Latitude != null) turbine.Latitude = (float)update.Latitude;
            if (update.Longitude != null) turbine.Longitude = (float)update.Longitude;

            await locations.UpdateAsync(turbine);
            return mapper.Map<LocationResponseDto?>(await locations.GetByIdAsync(id));
        }

        public async Task<bool> DeleteLocation(int id)
        {
            return await locations.DeleteAsync(id);
        }



    }
}
