using AutoMapper;
using Backend.Data.DTO;
using Backend.Data.DTO.Create;
using Backend.Data.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Services
{
    public class TaskItemService
    {
        private readonly IRepository<TaskItem> tasks;
        private readonly IMapper mapper;

        public TaskItemService(IRepository<TaskItem> tasks, IMapper mapper)
        {
            this.tasks = tasks;
            this.mapper = mapper;
        }

        public async Task<TaskItemDetailDto> CreateTaskItem(TaskItemCreateDto create)
        {
            TaskItem taskItem = new TaskItem(create.Name,create.DurationHours);

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


            return mapper.Map<TaskItemDetailDto>(await tasks.AddAsync(taskItem));
        }

        public async Task<List<TaskItemSummaryDto>> GetAll()
        {
            return mapper.Map<List<TaskItemSummaryDto>>(await tasks.GetAllAsync());
        }

        public async Task<TaskItemDetailDto?> GetTaskItem(int id)
        {
            return mapper.Map<TaskItemDetailDto?>(await tasks.GetByIdAsync(id));
        }

        public async Task<TaskItemDetailDto?> UpdateTaskItem(int id, TaskItemUpdateDto update)
        {
            var taskItem = await tasks.GetByIdAsync(id);

            if (taskItem == null) {
                return null;
            }
            if (update.Name != null) taskItem.Name = update.Name;
            if (update.DurationHours is float hours) taskItem.DurationHours = hours;

            if (update.RequiredTools != null)
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

            if (update.RequiredQualifications != null)
            {
                update.RequiredQualifications.Clear();
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

