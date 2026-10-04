# Architecture

Mo3RegUI is a single .NET Framework 4.0 WPF assembly. Inside that assembly the code is divided into four layers, each with its own folder and namespace.

| Folder | Namespace | Responsibility |
| --- | --- | --- |
| `MvvmContract` | `Mo3RegUI.MvvmContract` | The only surface that the `View` and the `ViewModel` share. |
| `ViewModel` | `Mo3RegUI.ViewModel` | All application logic and the background tasks. |
| `View` | `Mo3RegUI.View` | A dumb rendering layer. |
| `Exe` | `Mo3RegUI.Exe` | The composition root. |

## Layer boundaries

The `View` and the `ViewModel` never reference each other. They communicate only through `MvvmContract`, and `Exe` is the single place that knows both of them.

- The `View` binds to the observable properties of a contract interface and invokes its commands. It makes no decisions of its own and contains no business logic.
- The `ViewModel` computes everything the `View` displays and exposes it as properties and commands on the contract interface. It never touches a WPF control, a window or a dialog.
- Everything the `ViewModel` needs from the `View` (showing a dialog, opening a URL, closing the window) is requested through a `ViewServices` interface declared in `MvvmContract` and implemented in the `View` layer. `Exe` injects the implementation.

The dependency graph is therefore one-way, and `MvvmContract` depends on neither the `View` nor the `ViewModel`:

```
View        ---> MvvmContract
ViewModel   ---> MvvmContract
Exe         ---> View, ViewModel, MvvmContract
```

## MvvmContract

Holds only types that both sides may see:

- `IMainWindowViewModel` — the main window's contract. It exposes observable properties (`WindowTitle`, `Messages`, the save-button flags) and one `ICommand` per user gesture. It declares no methods and no events.
- `IMessageItem` — one line in the message list, as the `View` sees it.
- `MessageLevel` and `Constants` — shared values and constants.
- `Mvvm/` — the MVVM primitives. Because this is a .NET Framework 4.0 application, we cannot use `CommunityToolkit.Mvvm` package. Instead, `ObservableObject` and `RelayCommand` / `IRelayCommand` are implemented here.
- `ViewServices/` — the interfaces the `ViewModel` uses to reach the UI: `IDialogService`, `IUrlService` and `IViewLifecycleService`.

For the same reason, `ObservableObject.SetProperty` takes the property name explicitly (`nameof`) instead of using `CallerMemberName`, which does not exist in that framework version.

## ViewModel

Contains all of the logic:

- `MainWindowViewModel` decides which tasks run, collects their messages, exports the log and asks for confirmation before the window closes while tasks are running.
- `MessagesViewModel` and `MessageItemViewModel` hold the collected messages.
- `Localization` and `LogExporter` produce the localized text and the Markdown log.
- `Tasks/` holds the individual checks and fixes.
- `Infrastructure/` holds shared helpers such as INI access, native methods and file locks.

The `ViewModel` never references the `View` namespace.

## View

A dumb rendering layer:

- `MainWindow.xaml` — data bindings and commands only.
- `MainWindow.xaml.cs` — assigns the DataContext and nothing else.
- `Converters/` — value converters used by the XAML.
- `Services/` — the WPF implementations of the `ViewServices` interfaces (`DialogService`, `UrlService`, `ViewLifecycleService`).

The `View` never references the `ViewModel` namespace.

## Exe

The composition root, and the only layer that knows both the `View` and the `ViewModel`:

- `App.OnStartup` creates the `ViewServices` implementations and passes them to the `ViewModel`.
- It creates the window with the `ViewModel`, attaches the window lifecycle and starts the task run.
