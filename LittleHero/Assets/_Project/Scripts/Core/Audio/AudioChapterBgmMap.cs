namespace LittleHero
{
    /// <summary>5027 — chapter → BGM name 정적 매핑 (chapter_structure_v2 §3).</summary>
    public static class AudioChapterBgmMap
    {
        public static string Get(int chapterNumber)
        {
            string suffix = (chapterNumber % 2 == 0) ? "_a" : "_b";
            if (chapterNumber <= 5)  return "ch_meta"    + suffix;
            if (chapterNumber <= 12) return "ch_nature"  + suffix;
            if (chapterNumber <= 20) return "ch_animal"  + suffix;
            if (chapterNumber <= 28) return "ch_civ"     + suffix;
            return "ch_fantasy" + suffix;
        }
    }
}
