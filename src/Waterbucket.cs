using System;
using System.IO;
using System.Collections.Generic;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using Pipliz.Threading;
using Pipliz.APIProvider.Recipes;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class WaterbucketModEntries
  {
    public static string MOD_PREFIX = "mods.scarabol.waterbucket.";
    private static string AssetsDirectory;
    private static string RelativeTexturesPath;
    private static string RelativeIconsPath;

    [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAssemblyLoaded, "scarabol.waterbucket.assemblyload")]
    public static void OnAssemblyLoaded(string path)
    {
      string ModDirectory = Path.GetDirectoryName(path);
      AssetsDirectory = Path.Combine(ModDirectory, "assets");
      ModLocalizationHelper.localize(Path.Combine(AssetsDirectory, "localization"), MOD_PREFIX, false);
      // TODO this is realy hacky (maybe better in future ModAPI)
      RelativeTexturesPath = new Uri(MultiPath.Combine(Path.GetFullPath("gamedata"), "textures", "materials", "blocks", "albedo", "dummyfile")).MakeRelativeUri(new Uri(MultiPath.Combine(AssetsDirectory, "textures", "albedo"))).OriginalString;
      RelativeIconsPath = new Uri(MultiPath.Combine(Path.GetFullPath("gamedata"), "textures", "icons", "dummyfile")).MakeRelativeUri(new Uri(MultiPath.Combine(AssetsDirectory, "icons"))).OriginalString;
    }

    [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterStartup, "scarabol.waterbucket.registercallbacks")]
    public static void AfterStartup()
    {
      Pipliz.Log.Write("Loaded Waterbucket Mod 1.1 by Scarabol");
    }

    [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, "scarabol.waterbucket.addrawtypes")]
    public static void AfterAddingBaseTypes()
    {
      ItemTypesServer.AddTextureMapping(MOD_PREFIX + "bucketSide", new JSONNode()
        .SetAs("albedo", "furnaceSide")
        .SetAs("normal", "neutral")
        .SetAs("emissive", "neutral")
        .SetAs("height", "neutral")
      );
      ItemTypesServer.AddTextureMapping(MOD_PREFIX + "bucketemptytop", new JSONNode()
        .SetAs("albedo", Path.Combine(RelativeTexturesPath, "bucketEmptyTop"))
        .SetAs("normal", "furnaceUnlitTop")
        .SetAs("emissive", "neutral")
        .SetAs("height", "neutral")
      );
      ItemTypes.AddRawType(MOD_PREFIX + "bucket",
        new JSONNode(NodeType.Object)
          .SetAs("icon", Path.Combine(RelativeIconsPath, "bucket.png"))
          .SetAs("maxStackSize", 5)
          .SetAs("needsBase", "true")
          .SetAs("npcLimit", 0)
          .SetAs("sideall", MOD_PREFIX + "bucketSide")
          .SetAs("sidey+", MOD_PREFIX + "bucketemptytop")
      );
      ItemTypesServer.AddTextureMapping(MOD_PREFIX + "waterbucketfilledtop", new JSONNode()
        .SetAs("albedo", Path.Combine(RelativeTexturesPath, "waterbucketFilledTop"))
        .SetAs("normal", "furnaceUnlitTop")
        .SetAs("emissive", "neutral")
        .SetAs("height", "neutral")
      );
      ItemTypes.AddRawType(MOD_PREFIX + "waterbucket",
        new JSONNode(NodeType.Object)
          .SetAs("icon", Path.Combine(RelativeIconsPath, "waterbucket.png"))
          .SetAs("maxStackSize", 5)
          .SetAs("needsBase", "true")
          .SetAs("npcLimit", 0)
          .SetAs("sideall", MOD_PREFIX + "bucketSide")
          .SetAs("sidey+", MOD_PREFIX + "waterbucketfilledtop")
      );
    }

    [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesServer, "scarabol.waterbucket.registertypes")]
    public static void AfterItemTypesServer()
    {
      ItemTypesServer.RegisterOnRemove("water", WaterbucketCode.OnWaterRemoved);
      ItemTypesServer.RegisterOnAdd(MOD_PREFIX + "waterbucket", WaterbucketCode.OnAddFilled);
    }

    [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.waterbucket.loadrecipes")]
    [ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.registerrecipes")]
    public static void AfterItemTypesDefined()
    {
      RecipePlayer.AllRecipes.Add(new Recipe(new JSONNode()
        .SetAs("results", new JSONNode(NodeType.Array).AddToArray(new JSONNode().SetAs("type", MOD_PREFIX + "bucket")))
        .SetAs("requires", new JSONNode(NodeType.Array).AddToArray(new JSONNode().SetAs("type", "ironingot").SetAs("amount", 3)))
      ));
    }
  }

  static class WaterbucketCode
  {
    public static void OnWaterRemoved(Vector3Int position, ushort wasType, Players.Player causedBy)
    {
      ThreadManager.InvokeOnMainThread(delegate ()
      {
        ushort newType;
        if (World.TryGetTypeAt(position, out newType) && newType == ItemTypes.IndexLookup.GetIndex(WaterbucketModEntries.MOD_PREFIX + "bucket")) {
          if (ServerManager.TryChangeBlock(position, ItemTypes.IndexLookup.GetIndex("air"))) {
            Stockpile.GetStockPile(causedBy).Add(ItemTypes.IndexLookup.GetIndex(WaterbucketModEntries.MOD_PREFIX + "waterbucket"), 1);
            Chat.Send(causedBy, string.Format("water filled bucket added to your stockpile"));
          }
        }
      }, 0.5);
    }

    public static void OnAddFilled(Vector3Int position, ushort newType, Players.Player causedBy)
    {
      ThreadManager.InvokeOnMainThread(delegate ()
      {
        ushort actualType;
        if (World.TryGetTypeAt(position, out actualType) && actualType == ItemTypes.IndexLookup.GetIndex(WaterbucketModEntries.MOD_PREFIX + "waterbucket")) {
          Chat.SendToAll(string.Format("{0} spilled some water at {1}", causedBy.Name, position));
          if (ServerManager.TryChangeBlock(position, ItemTypes.IndexLookup.GetIndex("water"))) {
            Stockpile.GetStockPile(causedBy).Add(ItemTypes.IndexLookup.GetIndex(WaterbucketModEntries.MOD_PREFIX + "bucket"), 1);
            Chat.Send(causedBy, string.Format("bucket added to your stockpile"));
          }
        }
      }, 0.5);
    }
  }
}
