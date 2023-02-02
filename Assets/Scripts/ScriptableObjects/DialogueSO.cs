using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSO : ScriptableObject
{
    internal static readonly string[,] startDialogue = new string[,]
    {
        { "Koakuma", "Stop right there, criminal scum!" },
        { "Postulate", "My my, aren't you cultured? What's your name?" },
        { "Koakuma", "My name is Koakuma." },
        { "Postulate", "Are you coming to the festival too?" },
        { "Koakuma", "No, i'm here to return you!" },
        { "Postulate", "You think you can withstand the might of everyone in Gensokyo?!" },
        { "Koakuma", "I can try. I cannot disappoint my Lady." },
        { "Postulate", "Hmm, the festival is still not up for a few hours from now... I guess i can play with you for a bit." },
        { "Koakuma", "Bring it!" },
        { "Postulate", "I don't even need to recite the most powerful spell cards to defeat you! Face the wrath of..." },
        { "Postulate", "the Hourai Doll..." },
        { "Postulate", "Fairy of the Ice..." },
        { "Postulate", "and the Sealed Great Acharya of Youkais!" },
        { "Koakuma", "Pretty sure 2 out of 3 of them are bosses..." }
    };
    internal static readonly string[,] endDialogue = new string[,]
    {
        { "Koakuma", "That wasn't as hard as i thought." },
        { "Postulate", "Mukyuuu..." },
        { "Koakuma", "You're coming back with me!" },
        { "Postulate", "Aaa, please! Don't revert me back to a book! And i just want to have fun, that's all!" },
        { "", "Koakuma notices Postulate is starting to tear up..." },
        { "Koakuma", "I'm sorry but Lady Patchouli takes the final decision of your fate." },
        { "Postulate", "Don't do this to me! Please!" },
        { "Koakuma", "Maybe you can apologize to my Lady and talk things out with her." },
        { "Postulate", "She'd probably beat me up harder than you did..." },
        { "Koakuma", "My lady is not like that." },
        { "Koakuma", "Everyone in the mansion may seem like a scary bunch, but more friendly than you think." },
        { "Postulate", "... If this is my only way out then so be it." },
        { "", "Postulate later continues her life to be Patchouli's assistant alongside with Koakuma..." },
        { "", "and able to participate in Moriya Omatsuri." },
        { "", "She'll be fine (-w- )" },
    };
}
