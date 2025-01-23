using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class LevelRuleTile : RuleTile<LevelRuleTile.Neighbor>
{
    public bool customField;


    public TileBase[] friendTiles;

    public class Neighbor : RuleTile.TilingRule.Neighbor
    {
        public const int This = 1;

        public const int ThisOrFriend = 2;

        public const int NotThis = 3;
    }

    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        switch (neighbor)
        {
            case Neighbor.This: return tile == this;
            case Neighbor.ThisOrFriend:
                return tile == this
                     || HasFriendTile(tile);
            case Neighbor.NotThis: return !(tile == this
                     || HasFriendTile(tile));
        }
        return true;
    }

    private bool HasFriendTile(TileBase tile)
    {
        if (tile == null)
            return false;

        for (int i = 0; i < friendTiles.Length; i++)
        {
            if (friendTiles[i] == tile)
                return true;
        }
        return false;
    }
}