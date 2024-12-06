# SaveSafety

When a patch to RestoreGlobalData and SaveGlobalData explode, remaining patches are not executed. This means that your save could be partiallycorrupted/lost if it's saved afterwards.

When this happens it's likely to be missed because it does not stop the game from loading into a new scene.

This simple mod explicitly warn you when saving or loading patches are not completed succesfully.

![](./screenshot.png)
