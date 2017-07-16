using System;

public interface INotification<T>
{
    string ObserverName { get; set; }
    T Body { get; set; }
    bool Destroy { get; set; }
    bool IsUsing { get; set; }
}