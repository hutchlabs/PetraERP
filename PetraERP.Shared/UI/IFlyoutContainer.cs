using System.Collections.ObjectModel;

namespace PetraERP.Shared.UI
{
    public interface IFlyoutContainer
    {
        ObservableCollection<FlyoutViewModelBase> Flyouts { get; set; }
    }
}
