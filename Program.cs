using TidyHPC.Routers.Args;
using WindowsCommonCLI;
using WindowsCommonCLI.Windows;

Self.aesKey = Guid.Parse("A3739409-1DD2-4A6A-A4B6-BCC73D56C856");
Self.args = args;
ArgsRouter argsRouter = new();
argsRouter.Register(["get-window"], WindowCommands.GetWindow);
argsRouter.Register(["list-windows"], WindowCommands.List);
argsRouter.Register(["list-user-windows"], WindowCommands.ListUser);
argsRouter.Register(["list-children-windows"], WindowCommands.ListChildren);
argsRouter.Register(["click-window"], WindowCommands.Click);
argsRouter.Register(["right-click-window"], WindowCommands.RightClick);
argsRouter.Register(["set-window-text"], WindowCommands.SetText);
argsRouter.Register(["find-window-text", "find-window-title"], WindowCommands.FindWindowText);
argsRouter.Register(["find-child-window"], WindowCommands.FindChildWindow);
argsRouter.Register(["select-combobox-index"], WindowCommands.SelectComboboxIndex);
argsRouter.Register(["select-combobox-text"], WindowCommands.SelectComboboxText);
argsRouter.Register(["send-text"], WindowCommands.SendText);
argsRouter.Register(["send-keys"], WindowCommands.SendKeys);
argsRouter.Register(["active-english-keyboard"], WindowCommands.ActiveEnglishKeyboard);
argsRouter.Register(["match-window"], WindowCommands.MatchWindow);
argsRouter.Register(["close-window"], WindowCommands.CloseWindow);
argsRouter.Register(["mouse-move"], WindowCommands.MouseMove);
argsRouter.Register(["mouse-move-to"], WindowCommands.MouseMoveTo);
argsRouter.Register(["mouse-click"], WindowCommands.MouseClick);

// io
argsRouter.Register(["copy-directory"], IOCommands.CopyDirectory);
argsRouter.Register(["delete-directory"], IOCommands.DeleteDirectory);

argsRouter.Register(["extract"], CompressCommands.Extract);
argsRouter.Register(["compress"], CompressCommands.Compress);

argsRouter.Register(["markdown-increase"], MarkdownCommands.Increase);

argsRouter.Register(["kill-process"], ProcessCommands.Kill);

await argsRouter.Route(args);