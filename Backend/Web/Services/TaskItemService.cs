using AutoMapper;
using Backend.Data.DTO;
using Backend.Data.DTO.Create;
using Backend.Data.Entitites;
using Backend.Data.Repositories;

namespace Backend.Web.Services
{
	public class TaskItemService
	{
		private readonly ITaskItemRepository tasks;
		private readonly IMapper mapper;
		private readonly TurbineService turbineService;

		public TaskItemService(ITaskItemRepository tasks, IMapper mapper, TurbineService turbineService)
		{
			this.tasks = tasks;
			this.mapper = mapper;
			this.turbineService = turbineService;
		}

		public async Task<TaskItemDetailDto> CreateTaskItem(TaskItemCreateDto create)
		{
			TaskItem taskItem = new TaskItem(create.Name,create.DurationHours,create.ExecutionIntervalStart, create.ExecutionIntervalEnd);
			taskItem.IsCompleted = false;

			foreach (var reqTool in create.RequiredTools)
			{
				taskItem.RequiredTools.Add(new TaskTool
				{
					ToolId = reqTool.ToolId,
					RequiredAmount = reqTool.RequiredAmount
				});
			}

			foreach (var reqQual in create.RequiredQualifications)
			{
				taskItem.RequiredQualifications.Add(new TaskQualification
				{
					QualificationId = reqQual.QualificationId,
					RequiredAmount = reqQual.requiredAmount
				});
			}
			await tasks.AddAsync(taskItem);
			var saved = await tasks.GetFullByIdAsync(taskItem.Id);
			return mapper.Map<TaskItemDetailDto>(saved);
		}

		public async Task<List<TaskItemSummaryDto>> GetAll()
		{
			return mapper.Map<List<TaskItemSummaryDto>>(await tasks.GetAllAsync());
		}

		public async Task<TaskItemDetailDto?> GetTaskItem(int id)
		{
			return mapper.Map<TaskItemDetailDto?>(await tasks.GetFullByIdAsync(id));
		}

		public async Task<TaskItemDetailDto?> UpdateTaskItem(int id, TaskItemUpdateDto update)
		{
			var taskItem = await tasks.GetFullByIdAsync(id);

			if (taskItem == null) {
				return null;
			}
			if (update.Name != null) taskItem.Name = update.Name;
			if (update.DurationHours is float hours) taskItem.DurationHours = hours;
			if (update.IsCompleted != null) taskItem.IsCompleted = (bool)update.IsCompleted;
			if (update.ExecutionIntervalStart != null) taskItem.ExecutionIntervalStart = (DateOnly)update.ExecutionIntervalStart;
			if (update.ExecutionIntervalEnd != null) taskItem.ExecutionIntervalEnd = (DateOnly)update.ExecutionIntervalEnd;
			
			if (update.LocationId != null)
			{
				var turbine = await turbineService.GetTurbine((int)update.LocationId);
				if (turbine == null) throw new Exception($"Turbine with id ${update.LocationId}");
				update.LocationId = turbine.Id;
			}

			if (update.RequiredTools!.Count != 0)
			{
				taskItem.RequiredTools.Clear();
				foreach (var reqTool in update.RequiredTools)
				{
					taskItem.RequiredTools.Add(new TaskTool
					{
						ToolId = reqTool.ToolId,
						RequiredAmount = reqTool.RequiredAmount
					});
				}
			}

			if (update.RequiredQualifications!.Count != 0)
			{
				taskItem.RequiredQualifications.Clear();
				foreach (var reqQual in update.RequiredQualifications)
				{
					taskItem.RequiredQualifications.Add(new TaskQualification
					{
						QualificationId = reqQual.QualificationId,
						RequiredAmount = reqQual.requiredAmount
					});
				}
			}


			return mapper.Map<TaskItemDetailDto>(await tasks.UpdateAsync(taskItem));
		}

		public async Task<bool> DeleteTaskItem(int id)
		{
			return await tasks.DeleteAsync(id);
		}

	}
}

