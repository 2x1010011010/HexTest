using System.Collections.Generic;
using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.Boosters;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace HexaSortTest.CodeBase.GameLogic.UI.Shop
{
  [DisallowMultipleComponent]
  public sealed class ShopProductAuthoring : MonoBehaviour
  {
    [BoxGroup("PRODUCT"), SerializeField] private string _id = "starter_kit";
    [BoxGroup("PRODUCT"), SerializeField] private string _title = "Starter kit";
    [BoxGroup("PRODUCT"), SerializeField, TextArea] private string _description = "100 Coins + 1 Hammer";
    [BoxGroup("PRODUCT"), SerializeField] private Sprite _icon;
    [BoxGroup("PRODUCT"), SerializeField] private string _priceLabel = "Coming soon";
    [BoxGroup("REWARDS"), SerializeField, Min(0)] private int _coins = 100;
    [BoxGroup("REWARDS"), SerializeField, Min(0)] private int _hammer = 1;
    [BoxGroup("REWARDS"), SerializeField, Min(0)] private int _hand;
    [BoxGroup("REWARDS"), SerializeField, Min(0)] private int _respawn;
    [BoxGroup("REWARDS"), SerializeField, Tooltip("One-time: capacity and current lives become 10; records the future ad-removal entitlement.")]
    private bool _upgradeLives;
    [BoxGroup("OUTPUT"), SerializeField] private StorePositionBundle _product;
    [BoxGroup("OUTPUT"), SerializeField] private StorePositionBundleCatalog _catalog;
    [BoxGroup("PREVIEW"), SerializeField] private StorePositionBundleListItem _previewCard;

#if UNITY_EDITOR
    [Button("Validate"), BoxGroup("ACTIONS")]
    private bool ValidateDraft()
    {
      bool valid = !string.IsNullOrWhiteSpace(_id) && !string.IsNullOrWhiteSpace(_title) &&
        System.Text.RegularExpressions.Regex.IsMatch(_id, @"^[a-zA-Z0-9_.-]+$") &&
        _coins >= 0 && _hammer >= 0 && _hand >= 0 && _respawn >= 0 &&
        (_coins > 0 || _hammer > 0 || _hand > 0 || _respawn > 0 || _upgradeLives);
      if (_product != null && _product.Sku != _id) valid = false;
      if (_catalog != null && _catalog.Bundles != null)
        foreach (var other in _catalog.Bundles)
          if (other != null && other != _product && other.Sku == _id) valid = false;
      if (!valid) Debug.LogError("Use a unique stable ID, title and at least one positive reward. Load an existing product before editing it. To create a copy, clear Product and change ID.", this);
      return valid;
    }

    [Button("Load selected product"), BoxGroup("ACTIONS")]
    private void LoadProduct()
    {
      if (_product == null) return;
      Undo.RecordObject(this, "Load shop product");
      _id = _product.Sku; _title = _product.DisplayName; _description = _product.Description;
      _icon = _product.Icon; _priceLabel = _product.PriceDisplayFallback;
      _coins = _hammer = _hand = _respawn = 0; _upgradeLives = false;
      if (_product.Entries != null) foreach (var entry in _product.Entries)
      {
        if (entry?.StorePosition == null) continue;
        var p = entry.StorePosition; int amount = checked(p.Amount * entry.Quantity);
        if (p.Type == StorePositionType.Coins) _coins = checked(_coins + amount);
        if (p.Type == StorePositionType.LivesUpgrade) _upgradeLives = true;
        if (p.Type != StorePositionType.Booster) continue;
        switch (p.BoosterType)
        { case BoosterType.Hammer: _hammer = checked(_hammer + amount); break;
          case BoosterType.Hand: _hand = checked(_hand + amount); break;
          case BoosterType.Respawn: _respawn = checked(_respawn + amount); break; }
      }
      EditorUtility.SetDirty(this);
    }

    [Button("Save product + add to catalog"), BoxGroup("ACTIONS"), DisableInPlayMode]
    private void SaveProduct()
    {
      const string folder = "Assets/HexaSortTest/Resources/StaticData/Shop/Products";
      const string catalogPath = "Assets/HexaSortTest/Resources/StaticData/GameConfigs/StorePositionBundleCatalog.asset";
      if (_catalog == null) _catalog = AssetDatabase.LoadAssetAtPath<StorePositionBundleCatalog>(catalogPath);
      if (!ValidateDraft()) return;
      Directory.CreateDirectory(folder);
      if (_product == null)
      {
        string path = folder + "/" + _id + ".asset";
        if (File.Exists(path)) { Debug.LogError("A product at this path already exists. Select it and Load selected product.", this); return; }
        _product = ScriptableObject.CreateInstance<StorePositionBundle>();
        AssetDatabase.CreateAsset(_product, path);
      }
      Undo.RecordObject(this, "Save shop product");
      Undo.RecordObject(_product, "Edit shop product");
      string productPath = AssetDatabase.GetAssetPath(_product);
      var previous = new List<StorePosition>();
      if (_product.Entries != null) foreach (var e in _product.Entries)
        if (e?.StorePosition != null && AssetDatabase.IsSubAsset(e.StorePosition) &&
            AssetDatabase.GetAssetPath(e.StorePosition) == productPath) previous.Add(e.StorePosition);
      var rewards = new List<StorePosition>();
      AddReward(rewards, StorePositionType.Coins, _coins, BoosterType.Hammer);
      AddReward(rewards, StorePositionType.Booster, _hammer, BoosterType.Hammer);
      AddReward(rewards, StorePositionType.Booster, _hand, BoosterType.Hand);
      AddReward(rewards, StorePositionType.Booster, _respawn, BoosterType.Respawn);
      AddReward(rewards, StorePositionType.LivesUpgrade, _upgradeLives ? 1 : 0, BoosterType.Hammer);
      var so = new SerializedObject(_product);
      Set(so, "Sku", _id); Set(so, "DisplayName", _title); Set(so, "Description", _description);
      Set(so, "PriceDisplayFallback", _priceLabel);
      so.FindProperty("<Icon>k__BackingField").objectReferenceValue = _icon;
      var entries = so.FindProperty("<Entries>k__BackingField"); entries.arraySize = rewards.Count;
      for (int i = 0; i < rewards.Count; i++)
      {
        var e = entries.GetArrayElementAtIndex(i);
        e.FindPropertyRelative("_storePosition").objectReferenceValue = rewards[i];
        e.FindPropertyRelative("_quantity").intValue = 1;
      }
      so.ApplyModifiedProperties();
      foreach (var old in previous) Undo.DestroyObjectImmediate(old);
      if (_catalog == null)
      {
        Directory.CreateDirectory(Path.GetDirectoryName(catalogPath));
        _catalog = ScriptableObject.CreateInstance<StorePositionBundleCatalog>();
        AssetDatabase.CreateAsset(_catalog, catalogPath);
      }
      var cat = new SerializedObject(_catalog);
      var products = cat.FindProperty("<Bundles>k__BackingField"); bool found = false;
      for (int i = 0; i < products.arraySize; i++) if (products.GetArrayElementAtIndex(i).objectReferenceValue == _product) found = true;
      if (!found) { int i = products.arraySize++; products.GetArrayElementAtIndex(i).objectReferenceValue = _product; }
      cat.ApplyModifiedProperties();
      EditorUtility.SetDirty(this); EditorUtility.SetDirty(_product); EditorUtility.SetDirty(_catalog);
      AssetDatabase.SaveAssets();
      Preview();
      Debug.Log("Product saved. Catalog order controls the vertical list order.", this);
    }
    private void AddReward(List<StorePosition> rewards, StorePositionType type, int count, BoosterType booster)
    {
      if (count <= 0) return;
      var item = ScriptableObject.CreateInstance<StorePosition>(); item.name = type == StorePositionType.Booster ? booster.ToString() : type.ToString();
      AssetDatabase.AddObjectToAsset(item, _product); Undo.RegisterCreatedObjectUndo(item, "Create shop reward");
      var so = new SerializedObject(item); Set(so, "Id", _id + "." + item.name); Set(so, "DisplayName", item.name);
      so.FindProperty("<Type>k__BackingField").intValue = (int)type;
      so.FindProperty("<Amount>k__BackingField").intValue = count;
      so.FindProperty("<BoosterType>k__BackingField").intValue = (int)booster;
      so.ApplyModifiedProperties(); rewards.Add(item);
    }
    private static void Set(SerializedObject so, string field, string value) =>
      so.FindProperty("<" + field + ">k__BackingField").stringValue = value;
    [Button("Preview saved product"), BoxGroup("ACTIONS")]
    private void Preview()
    {
      if (_previewCard != null && _product != null)
      { _previewCard.Setup(_product); EditorUtility.SetDirty(_previewCard); }
    }
#endif
  }
}
