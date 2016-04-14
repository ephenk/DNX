using System;

namespace DNXServer
{
	public enum CommandResponseEnum
	{
		None, //0
		Error, //1
		Maintenance, //2
		Announcement, //3
		LoginResult, //4
		LoadCard, //5
		EditCard, //6
		RemoveCard, //7
		RegisterCardResult, //8
		UnregisterCardResult, //9
		UpgradeMonsterResult, //10
		BattleResult, //11
		MonsterInventory, //12
		RoomListResult, //13
		CreateRoomResult, //14
		JoinRoomResult, //15 
		SetBattleItemResult, //16
		BattleReadyResult, //17
		StartOnlineBattleResult, //18
		SendBattleActionResult, //19
		SuddenDeathResult, //20
		KickOpponentResult, //21
		QuitRoomResult, //22
		PurchaseResult, //23
		LoadShopItems, //24
		BuyShopItemResult, //25
		BuyBattleSlotResult //26
	}
}

