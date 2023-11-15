using Excel.Core.Entities;

namespace Excel.Infrastructure.Notification;

public interface ICellUpdateNotificationService
{
    public void Notify(Cell cell);

    public void Subscribe(Cell cell, Uri notificationUri);
}