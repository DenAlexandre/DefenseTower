using CommunityToolkit.Mvvm.Input;
using DefenseTowerOrigin.Models;

namespace DefenseTowerOrigin.PageModels;

public interface IProjectTaskPageModel
{
	IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
	bool IsBusy { get; }
}