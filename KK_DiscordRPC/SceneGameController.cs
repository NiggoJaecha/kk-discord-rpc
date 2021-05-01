using System;
using KKAPI.MainGame;

namespace KK_DiscordRPC {
	public class SceneGameController: GameCustomFunctionController
	{
		protected override void OnStartH(HSceneProc proc, bool freeH)
		{
			KoiDiscordRpc.hProc = proc;
			KoiDiscordRpc.currentStamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
		}
	}
}