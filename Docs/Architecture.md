# Architecture

Mo3RegUI is a single .NET Framework 4.0 WPF assembly. Inside that assembly the code is divided into four layers, each with its own folder and namespace.

| Folder | Namespace | Responsibility |
| --- | --- | --- |
| `MvvmContract` | `Mo3RegUI.MvvmContract` | The only surface that the View and the ViewModel share. |
| `ViewModel` | `Mo3RegUI.ViewModel` | All application logic and the background tasks. |
| `View` | `Mo3RegUI.View` | A dumb rendering layer. |
| `Exe` | `Mo3RegUI.Exe` | The composition root. |

## The rule

The View and the ViewModel never reference each other. They communicate only through `MvvmContract`, and `Exe` is the single place that knows both of them.

- The View binds to the observable properties of a contract interface and invokes its commands. It makes no decisions of its own and contains no business logic.
- The ViewModel computes everything the View displays and exposes it as properties and commands on the contract interface. It never touches a WPF control, a window or a dialog.
- Everything the ViewModel needs from the View (showing a dialog, opening a URL, closing the window) is requested through a `ViewServices` interface declared in `MvvmContract` and implemented in the `View` layer. `Exe` injects the implementation.

## MvvmContract

Holds only types that both sides may see:

- `IMainWindowViewModel` — the main window's contract. It exposes observable properties (`WindowTitle`, `Messages`, the save-button flags) and one `ICommand` per user gesture. It declares no methods and no events.
- `IMessageItem` — one line in the message list, as the View sees it.
- `MessageLevel` and `Constants` — shared values and constants.
- `Mvvm/` — the MVVM primitives. The layer takes no NuGet dependency, so `ObservableObject` (a minimal `INotifyPropertyChanged` base) and `RelayCommand` / `IRelayCommand` are implemented here rather than supplied by a toolkit.
- `ViewServices/` — the interfaces the ViewModel uses to reach the UI: `IDialogService`, `IUrlService` and `IViewLifecycleService`.

Because the target framework is .NET Framework 4.0, `ObservableObject.SetProperty` takes the property name explicitly (`nameof`) instead of using `CallerMemberName`, which does not exist in that framework version.

## ViewModel

Contains all of the logic:

- `MainWindowViewModel` decides which tasks run, collects their messages, exports the log and asks for confirmation before the window closes while tasks are running.
- `MessagesViewModel` and `MessageItemViewModel` hold the collected messages.
- `Localization` and `LogExporter` produce the localized text and the Markdown log.
- `Tasks/` holds the individual checks and fixes.
- `Infrastructure/` holds shared helpers such as INI access, native methods and file locks.

## View

A dumb rendering layer:

- `MainWindow.xaml` — data bindings and commands only.
- `MainWindow.xaml.cs` — assigns the DataContext and nothing else.
- `Converters/` — value converters used by the XAML.
- `Services/` — the WPF implementations of the `ViewServices` interfaces (`DialogService`, `UrlService`, `ViewLifecycleService`).

The View never references the `ViewModel` namespace.

## Exe

The composition root. `App.OnStartup` creates the View services, creates the ViewModel with them, creates the window with the ViewModel, attaches the window lifecycle and starts the task run. It is the only layer that references both `View` and `ViewModel`.

## Dependency direction

```
View        ---> MvvmContract
ViewModel   ---> MvvmContract
Exe         ---> View, ViewModel, MvvmContract
```

`MvvmContract` depends on neither `View` nor `ViewModel`. There is no reference from the View to the ViewModel, and none from the ViewModel to the View.
