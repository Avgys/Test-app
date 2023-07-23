using System;
using UnityEngine;

namespace Figure
{
    [Serializable]
    public class FigureType
    {
        public enum FigureTypeEnum
        {
            Red,
            Green,
            Blue,
            Yellow
        }

        public FigureTypeEnum Type;
        public Color Color;
    }

    [Serializable]
    public class Team
    {
        public int Id;

        public enum TeamEnum
        {
            Zero = 0,
            Team1 = 1,
            Team2 = 2,
        }

        public TeamEnum TeamName
        {
            get => (TeamEnum)Id;
            set => Id = (int)value;
        }

        public Color Color;
    }
}