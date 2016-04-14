using System;

namespace DNXServer
{
	public enum CommandEnum
	{
		None, // 0
		SendUserData, // 1
		RetrieveUserCards, // 2
		RegisterCard, // 3
		UnregisterCard, // 4
		UpgradeMonster, // 5
		SendBattleData, // 6
		SyncCardData, // 7
		PettingAction, // 8
		DropPettingStat, // 9
		RetrieveMonsterInventory, // 10
		UpdateMonsterEquipment, // 11
		RetrieveShopItems, // 12
		VerifyPurchase, // 13
		BuyShopItem, // 14
		BuyBattleSlot, //15
		RoomList, // 16
		CreateRoom, // 17
		JoinRoom, // 18
		KickOpponent, // 19
		SetBattleItem, // 20
		BattleReady, // 21
		StartOnlineBattle, // 22
		BattleAction, // 23
		SuddenDeath, // 24
		FinishBattle, // 25
		QuitRoom, // 26
		Logout // 27	

	}
}

