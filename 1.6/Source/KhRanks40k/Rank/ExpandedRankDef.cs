using Core40k;
using UnityEngine;

namespace KhRanks40k;

public class ExpandedRankDef : RankDef
{
    public Vector2 requiredAgeRange = new(-1, -1);
    public bool slaveRank = false;
}