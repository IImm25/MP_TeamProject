using AutoMapper;
using Backend.Data.DTO.Turbine;
using Backend.Data.Entitites;
using Backend.Data.Repositories;

namespace Backend.Web.Services
{
    public class TurbineService
    {
        private readonly IRepository<Turbine> turbines;
        private readonly IMapper mapper;

        public TurbineService(IRepository<Turbine> turbines, IMapper mapper)
        {
            this.turbines = turbines;
            this.mapper = mapper;
        }

        public async Task<TurbineResponseDto> CreateTurbine(TurbineCreateDto createInfo)
        {
            Turbine tur = new Turbine(createInfo.Name,createInfo.Latitude,createInfo.Longitude);
            return mapper.Map<TurbineResponseDto>(await turbines.AddAsync(tur));
        }

        public async Task<List<TurbineResponseDto>> GetAll()
        {
            return mapper.Map<List<TurbineResponseDto>>(await turbines.GetAllAsync());
        }

        public async Task<TurbineResponseDto?> GetTurbine(int id)
        {
            return mapper.Map<TurbineResponseDto?>(await turbines.GetByIdAsync(id));
        }

        public async Task<TurbineResponseDto?> UpdateTurbine(int id, TurbineUpdateDto update)
        {
            var turbine = await turbines.GetByIdAsync(id);
            if (turbine == null)
            {
                return null;
            }
            if (update.Name != null) turbine.Name = update.Name;
            if (update.Latitude != null) turbine.Latitude = (float)update.Latitude;
            if (update.Longitude != null) turbine.Longitude = (float)update.Longitude;

            return mapper.Map<TurbineResponseDto?>(await turbines.UpdateAsync(turbine));
        }

        public async Task<bool> DeleteTurbine(int id)
        {
            return await turbines.DeleteAsync(id);
        }



    }
}
