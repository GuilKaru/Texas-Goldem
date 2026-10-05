using System;
using UnityEngine;

namespace TexasHoldem
{
    [Serializable]
    public class AvatarEntry
    {
        public string slug;    // e.g. "mr_beast" — must match backend exactly
        public Sprite sprite;
    }

    [CreateAssetMenu(fileName = "AvatarRegistry", menuName = "TexasHoldem/Avatar Registry")]
    public class AvatarRegistry : ScriptableObject
    {
        [SerializeField] private AvatarEntry[] entries = new AvatarEntry[12];

        [Tooltip("Returned when a slug is not found or the sprite slot is empty.")]
        [SerializeField] private Sprite fallbackSprite;
        
        /// Returns the Sprite for the given agent slug
        /// Returns fallbackSprite if the slug is not found or the slot is unassigned.
        public Sprite GetSprite(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                Debug.LogWarning("[AvatarRegistry] GetSprite called with null/empty slug. Using fallback.");
                return fallbackSprite;
            }

            if (entries != null)
            {
                foreach (AvatarEntry entry in entries)
                {
                    if (entry != null &&
                        string.Equals(entry.slug, slug, StringComparison.OrdinalIgnoreCase))
                    {
                        if (entry.sprite != null)
                            return entry.sprite;

                        Debug.LogWarning($"[AvatarRegistry] Slug '{slug}' found but sprite is unassigned. Using fallback.");
                        return fallbackSprite;
                    }
                }
            }

            Debug.LogWarning($"[AvatarRegistry] Slug '{slug}' not found in registry. Using fallback.");
            return fallbackSprite;
        }

        // Editor validation
        private void OnValidate()
        {
            if (entries != null && entries.Length != 12)
            {
                Array.Resize(ref entries, 12);
                Debug.Log("[AvatarRegistry] Resized entries array to 12.");
            }
        }
        
        public string GetDisplayName(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return "Unknown";

            string[] parts = slug.Split('_');
            for (int i = 0; i < parts.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(parts[i]))
                    continue;

                string lower = parts[i].ToLower();
                parts[i] = char.ToUpper(lower[0]) + lower.Substring(1);
            }

            return string.Join(" ", parts);
        }
        
    }
}