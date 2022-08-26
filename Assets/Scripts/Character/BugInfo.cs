public enum BugFaction
{
  Wanderer = 0,
  Clergy = 1,
  Divine = 2,
  Imperial = 3,
  Ghost = 4,
  Scholar = 5,
  NightSenate = 6,
  Witch = 7
}
public enum BugSpecies
{
  Ant = 0,
  AssassinBug = 1,
  Bee = 2,
  Blowfly = 3,
  BombardierBeetle = 4,
  Booklouse = 5,
  Butterfly = 6,
  Cicada = 7,
  Cockroach = 8,
  CuckooWasp = 9,
  Dragonfly = 10,
  Firefly = 11,
  GiantHornet = 12,
  GoliathBeetle = 13,
  Ladybug = 14,
  Mantis = 15,
  Mayfly = 16,
  MoleCricket = 17,
  Mosquito = 18,
  Moth = 19,
  Scarab = 20,
  Shieldbug = 21,
  StickInsect = 22,
  Strepsiptera = 23,
  Termite = 24,
  Wasp = 25,
  WaterBoatman = 26,
  WaterBug = 27,
  WaterStrider = 28,
  Default = 1000,
}

[System.Serializable]
public class BugInfo
{
  public BugFaction faction;
  public BugSpecies species;

}