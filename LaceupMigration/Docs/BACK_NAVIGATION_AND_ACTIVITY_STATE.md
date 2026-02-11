# Back Navigation and Activity State (MAUI)

This doc describes how back navigation and activity state work so that restoration (e.g. after process death) stays in sync with the Shell stack. It mirrors the Xamarin pattern where every activity (here, every `LaceupContentPage`) participates in state and back in one place.

## One place per screen: `LaceupContentPage` + route name

- **Physical back** (Android device back) and **navigation bar back** both go through **`LaceupContentPage`**:
  - `OnBackButtonPressed()` calls `GoBack()` and returns `true`.
  - `BackButtonBehavior` command also calls `GoBack()`.
- **`GoBack()`** (base): calls `RemoveNavigationState()` then `Shell.Current.GoToAsync("..")`.
- **`RemoveNavigationState()`** uses **`GetRouteName()`** to know which route to remove from `ActivityState`.

So for each screen there is a single route name (e.g. `"ordercredit"`, `"productcatalog"`). That name is used when:
- Removing state on back (so we don’t leave stale state and re-push the page on restore).
- Optionally when saving state (e.g. in `ApplyQueryAttributes`); `NavigationTracker` also saves on forward navigation.

## Adding a new screen

1. **Use `LaceupContentPage`**  
   Your page should inherit from `LaceupContentPage` (or a subclass).

2. **Declare the route name**  
   Override `GetRouteName()` and return the Shell route for this page (no query string):

   ```csharp
   protected override string? GetRouteName() => "myroute";
   ```

   Register the route in `AppShell` (e.g. `Routing.RegisterRoute("myroute", typeof(MyPage));`) and add it to:
   - `NavigationHelper.RouteToActivityTypeMap` (in `NavigationHelper.cs`)
   - `ActivityStateRestorationService.ActivityTypeToRouteMap` (in `ActivityStateRestorationService.cs`)

3. **Default back behavior**  
   If you don’t override `GoBack()`, back (physical or nav bar) will:
   - Remove this page’s state (using `GetRouteName()`).
   - Pop with `GoToAsync("..")`.

4. **“Confirm before leaving” (e.g. unsaved changes)**  
   Override `GoBack()`, run your check (e.g. call ViewModel `OnBackButtonPressed()`), and only call `base.GoBack()` when the user is allowed to leave:

   ```csharp
   protected override async void GoBack()
   {
       if (await _viewModel.OnBackButtonPressed())
           return;
       base.GoBack();
   }
   ```

5. **“Back” does something other than a single pop**  
   Override `GoBack()`, call `RemoveNavigationState()` (so this page’s state is still removed), then do your navigation (e.g. call ViewModel method that navigates):

   ```csharp
   protected override void GoBack()
   {
       RemoveNavigationState();
       _ = _viewModel.DoneAsync();
   }
   ```

6. **Programmatic exit from ViewModel (Done, Save & close, etc.)**  
   To leave the screen and keep state correct, use:

   ```csharp
   await NavigationHelper.GoBackFromAsync("myroute");
   ```

   That removes this page’s state and pops once. For “pop twice” (e.g. ProductCatalog on top of FullCategory):

   ```csharp
   await NavigationHelper.GoBackFromAsync("productcatalog", "fullcategory");
   ```

   Prefer `GoBackFromAsync` instead of calling `RemoveNavigationState(...)` and `GoToAsync("..")` separately so all exits stay consistent.

## Summary

| Entry point              | Who handles it                          |
|--------------------------|-----------------------------------------|
| Physical back            | `OnBackButtonPressed` → `GoBack()`      |
| Navigation bar back      | `BackButtonBehavior` → `GoBack()`      |
| Programmatic exit (VM)   | `NavigationHelper.GoBackFromAsync(route)` |

Each screen defines its route once via `GetRouteName()`. Back (both buttons) and programmatic exit then remove that route’s state and navigate in a single, consistent way.
