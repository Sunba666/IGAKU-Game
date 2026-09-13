public static class LocalVersusSelectionStore
{
    public static CharacterSelectCharacterData Player1Character { get; private set; }
    public static CharacterSelectCharacterData Player2Character { get; private set; }

    public static void Save(
        CharacterSelectCharacterData player1Character,
        CharacterSelectCharacterData player2Character)
    {
        Player1Character = player1Character;
        Player2Character = player2Character;
    }

    public static void Clear()
    {
        Player1Character = null;
        Player2Character = null;
    }
}
