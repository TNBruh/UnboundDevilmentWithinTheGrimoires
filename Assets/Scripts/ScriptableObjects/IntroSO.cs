using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroSO : ScriptableObject
{
    internal static readonly string[,] dialogues = new string[,]
    {
        { "", "Omatsuri... in Gensokyo, it is also known as \"kami asobi\". A festival where gods get to play with the residents" },
        { "", "Today is the annual Moriya Omatsuri. Inhabitants of Gensokyo are all invited to play with the Moriya gods." },
        { "", "Naturally, \"playing\" is commonly equivalent to \"showering each other with danmakus\" in this world." },
        { "", "Somewhere in Gensokyo..." },
        { "Bored spirit",  "I’m bored. Is there anything that we can do?" },
        { "Sleepy spirit", "You can go attend the Moriya's kami asobi." },
        { "Doubtful spirit", "Something that’s not overwhelming for us." },
        { "Bored spirit", "Hmm..." },
        { "Doubtful spirit", "Don’t tell me you’re actually going there." },
        { "Bored spirit", "Maybe..." },
        { "", "The bored spirit flies away to the Scarlet Devil Mansion..." },
        { "Sleepy spirit", "Is she actually attending the omatsuri?" },
        { "Doubtful spirit", "It would be funny to see her get wrecked in the festival though." },
        { "Sleepy spirit", "Hah, let’s watch that later. I want to nap for now." },
        { "", "Inside the Scarlet Devil Mansion’s library..." },
        { "Bored spirit", "Hmm... where is it? Where’s the Thousand Spells Grimoire?" },
        { "Patchouli", "Hey! You! You’re not supposed to be here!" },
        { "Bored spirit", "Too late. I'll be permanently borrowing this! Bleeeh!" },
        { "", "The spirit possesses a grimoire and turns into a tsukumogami..." },
        { "Bored spirit", "Yes! This is more than enough to defeat everyone in the festival!" },
        { "", "Patchouli gets thrown away by the shockwave..." },
        { "Postulate \nWayward Tsukumogami of the Library", "As a token of gratitude, let me introduce myself. My name is Postulate. An escaped spirit of old makai." },
        { "Patchouli", "Uuugh..." },
        { "Postulate", "Stunning first impression, i know. Well… see you at the Moriya Omatsuri." },
        { "", "Postulate quickly leaves..." },
        { "Koakuma", "What was that noise?! Mistress?!" },
        { "Patchouli", "Koakuma, you have to get her. Return the archive grimoire back to the library." },
        { "Koakuma", "But mistress, you’re hurt. Let me-" },
        { "Patchouli", "I can take care of myself... I’m not dying or anything. Just go." },
        { "Koakuma", "R-right away!" },
        { "", "Koakuma gives chase..." },
        { "Patchouli", "...At least Remi’s mansion didn’t blow up this time." },
    };
    internal static readonly float readTime = 2.4f;
    internal static readonly float alphaChangeSpeed = 1.4f;
}
