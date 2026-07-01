using AutoMapper;
using Backend.Data.DTO.Plan;
using Backend.Data.DTO.TaskItem;
using Backend.Data.Entitites;
using Backend.Data.Repositories;

namespace Backend.Web.Services
{
    public class TaskItemService
    {
        private readonly ITaskItemRepository tasks;
        private readonly IMapper mapper;
        private readonly IRepository<Location> locations;

        public TaskItemService(ITaskItemRepository tasks, IMapper mapper, IRepository<Location> locations)
        {
            this.tasks = tasks;
            this.mapper = mapper;
            this.locations = locations;
        }

        public async Task<TaskItemDetailDto> CreateTaskItem(TaskItemCreateDto create)
        {
            TaskItem taskItem = new TaskItem(create.Name, create.DurationHours, create.ExecutionIntervalStart, create.ExecutionIntervalEnd);
            taskItem.IsCompleted = false;

            var location = await locations.GetByIdAsync(create.LocationId);
            if (location == null) throw new Exception($"Turbine with id ${create.LocationId}");
            taskItem.Location = location;

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
                    RequiredAmount = reqQual.RequiredAmount
                });
            }
            int id = await tasks.AddAsync(taskItem);
            return mapper.Map<TaskItemDetailDto>(await tasks.GetFullByIdAsync(id));
        }

        public async Task<List<TaskItemSummaryDto>> GetAll()
        {
            return mapper.Map<List<TaskItemSummaryDto>>(await tasks.GetAllAsync());
        }

        public async Task<TaskItemDetailDto?> GetTaskItem(int id)
        {
            return mapper.Map<TaskItemDetailDto?>(await tasks.GetFullByIdAsync(id));
        }

        public async Task<SingleTaskScheduleDto?> GetScheduleByTaskId(int id)
        {
            return mapper.Map<SingleTaskScheduleDto?>(await tasks.GetTaskScheduleByTaskIdAsync(id));
        }


        public async Task<TaskItemDetailDto?> UpdateTaskItem(int id, TaskItemUpdateDto update)
        {
            var taskItem = await tasks.GetFullByIdAsync(id);

            if (taskItem == null)
            {
                return null;
            }
            if (update.Name != null) taskItem.Name = update.Name;
            if (update.DurationHours is float hours) taskItem.DurationHours = hours;
            if (update.IsCompleted != null) taskItem.IsCompleted = (bool)update.IsCompleted;
            if (update.ExecutionIntervalStart != null) taskItem.ExecutionIntervalStart = (DateOnly)update.ExecutionIntervalStart;
            if (update.ExecutionIntervalEnd != null) taskItem.ExecutionIntervalEnd = (DateOnly)update.ExecutionIntervalEnd;

            if (update.LocationId != null)
            {
                var turbine = await locations.GetByIdAsync((int)update.LocationId);
                if (turbine == null) throw new Exception($"Turbine with id ${update.LocationId}");
                taskItem.Location = turbine;
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
                        RequiredAmount = reqQual.RequiredAmount
                    });
                }
            }

            await tasks.UpdateAsync(taskItem);
            return mapper.Map<TaskItemDetailDto>(await tasks.GetFullByIdAsync(id));
        }

        public async Task<bool> DeleteTaskItem(int id)
        {
            return await tasks.DeleteAsync(id);
        }

    }
}

