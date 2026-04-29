using System;
using AutoMapper;
using Backend.Data.DTO;
using Backend.Data.DTO.Create;
using Backend.Data.Repositories;

namespace Backend.Web.Services
{
    public class ToolService
    {
        private readonly IRepository<Tool> tools;
        private readonly ITaskItemRepository taskItems;
        private readonly IMapper mapper;

        public ToolService(IRepository<Tool> tools, ITaskItemRepository taskItems, IMapper mapper)
        {
            this.tools = tools;
            this.taskItems = taskItems;
            this.mapper = mapper;
        }

        public async Task<ToolResponseDto> CreateTool(ToolCreateDto create)
        {
            Tool tool = new Tool(create.Name, create.AvailableStock);
            return mapper.Map<ToolResponseDto>(await tools.AddAsync(tool));
        }

        public async Task<List<ToolResponseDto>> GetAll()
        {
            return mapper.Map<List<ToolResponseDto>>(await tools.GetAllAsync());
        }

        public async Task<ToolResponseDto?> GetTool(int id)
        {
            return mapper.Map<ToolResponseDto?>(await tools.GetByIdAsync(id));
        }

        public async Task<ToolResponseDto?> UpdateTool(int id, ToolUpdateDto update)
        {
            var tool = await tools.GetByIdAsync(id);
            if (tool == null)
            {
                return null;
            }
            if (update.Name != null) tool.Name = update.Name;
            if (update.AvailableStock is int stock) tool.AvailableStock = stock;

            return mapper.Map<ToolResponseDto?>(await tools.UpdateAsync(tool));
        }

        public async Task<bool> DeleteTool(int id)
        {
            return await tools.DeleteAsync(id);
        }

        public async Task<List<int>> GetTaskToolRequirements(int taskId)
        {
            var task = await taskItems.GetFullByIdAsync(taskId);
            if (task == null) return [];

            var requiredTools = new Dictionary<int, int>();

            foreach (var taskTools in task.RequiredTools)
            {
                requiredTools.Add(taskTools.ToolId, taskTools.RequiredAmount);
            }

            var quals = await tools.GetAllAsync();
            var allQualIds = quals.Select(qual => qual.Id).ToList();

            return allQualIds.Select(id => requiredTools.GetValueOrDefault(id)).ToList();
        }
    }
}
