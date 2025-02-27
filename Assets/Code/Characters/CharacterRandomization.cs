using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterRandomization {
    private static CharacterRandomization instance;
    private CharacterSerializer _characterSerializer;
    private CharacterResourceCollection _spriteCollection;
    private List<Color> _skinColors;

    public static CharacterRandomization Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CharacterRandomization();
            }
            return instance;
        }
    }

    private CharacterRandomization()
    {
        this._characterSerializer = CharacterSerializer.Instance;
        this._spriteCollection = GameObject.Find("CONTROLLER").GetComponent<CharacterResourceCollection>();
        this._skinColors = new List<Color>();
        this.LoadSkinColors();

        if (!this._characterSerializer.Initialized)
        {
            var newProperties = this.GetFullRandomCharacter();
            // TODO: As part of story mode, change happiness and fitness here
            // newProperties.happinessLevel = 4;
            // newProperties.fitnessLevel = 4;
            this._characterSerializer.CurrentCharacterProperties = newProperties;
        }
    }

	public CharacterProperties GetFullRandomCharacter(Gender gender = Gender.Male)
    {
		var newProperties = new CharacterProperties ();
		if (gender == Gender.Male)
        {
            newProperties.eyeSprite = this.GetRandomMaleEyeSprite();
            newProperties.hairSprite = this.GetRandomMaleHairSprite();
            newProperties.gender = Gender.Male;
        }
        else
        {
            newProperties.eyeSprite = this.GetRandomFemaleEyeSprite();
            newProperties.hairSprite = this.GetRandomFemaleHairSprite();
            newProperties.gender = Gender.Female;
        }
        newProperties.hairColor = new SerializableColor(CharacterRandomization.GetRandomColor());
        newProperties.shirtColor = new SerializableColor(CharacterRandomization.GetRandomColor());
        newProperties.pantsColor = new SerializableColor(CharacterRandomization.GetRandomColor());
        newProperties.skinColor = new SerializableColor(this.GetRandomSkinColor(Color.cyan));
        newProperties.birthmark = this.GetRandomBirthMark();
        return newProperties;
    }

    public string GetRandomMaleEyeSprite(string oldSprite = "")
    {
        var eyeSprites = this._spriteCollection.MaleEyeSprites;
        var finalSprite = oldSprite;
        if (eyeSprites.Count > 1)
        {
            while (finalSprite == oldSprite)
            {
                finalSprite = eyeSprites[UnityEngine.Random.Range(0, eyeSprites.Count)];
            }
        }
        return eyeSprites[0];
    }
    public string GetRandomFemaleEyeSprite(string oldSprite = "")
    {
        var eyeSprites = this._spriteCollection.FemaleEyeSprites;
        var finalSprite = oldSprite;
        if (eyeSprites.Count > 1)
        {
            while (finalSprite == oldSprite)
            {
                finalSprite = eyeSprites[UnityEngine.Random.Range(0, eyeSprites.Count)];
            }
        }
        return eyeSprites[0];
    }

    public string GetRandomMaleHairSprite(string oldSprite = "")
    {
        var hairSprites = this._spriteCollection.MaleHairSprites;
        var finalSprite = oldSprite;
        if (hairSprites.Count > 1)
        {
            while (finalSprite == oldSprite)
            {
                finalSprite = hairSprites[UnityEngine.Random.Range(0, hairSprites.Count)].name;
            }
        }
        return hairSprites[0].name;
    }
    public string GetRandomFemaleHairSprite(string oldSprite = "")
    {
        var hairSprites = this._spriteCollection.FemaleHairSprites;
        var finalSprite = oldSprite;
        if (hairSprites.Count > 1)
        {
            while (finalSprite == oldSprite)
            {
                finalSprite = hairSprites[UnityEngine.Random.Range(0, hairSprites.Count)].name;
            }
        }
        return hairSprites[0].name;
    }

    public string GetNextMaleHairSprite(string oldSprite = "")
    {
        var index = this._spriteCollection.MaleHairSprites.FindIndex(s => s.name == oldSprite);
        if (index != -1)
        {
            var nextIndex = this._spriteCollection.MaleHairSprites.Count == (index + 1) ? 0 : (index + 1);
            return this._spriteCollection.MaleHairSprites[nextIndex].name;
        }

        return this._spriteCollection.MaleHairSprites[0].name;
    }
    public string GetNextFemaleHairSprite(string oldSprite = "")
    {
        var index = this._spriteCollection.FemaleHairSprites.FindIndex(s => s.name == oldSprite);
        if (index != -1)
        {
            var nextIndex = this._spriteCollection.FemaleHairSprites.Count == (index + 1) ? 0 : (index + 1);
            return this._spriteCollection.FemaleHairSprites[nextIndex].name;
        }

        return this._spriteCollection.FemaleHairSprites[0].name;
    }

    public BirthMarkType GetRandomBirthMark()
    {
        var randomNumber = UnityEngine.Random.Range(0, 5);
        switch (randomNumber)
        {
            case 0:
                return BirthMarkType.MiddleMole;
            case 1:
                return BirthMarkType.LeftMole;
            case 2:
                return BirthMarkType.BottomMole;
            case 3:
                return BirthMarkType.RightMole;
            case 4:
                return BirthMarkType.TopMole;
            default:
                return BirthMarkType.None;
        }
    }

    public static Color GetRandomColor()
    {
        return new Color(
            UnityEngine.Random.Range(0.0f, 1.0f),
            UnityEngine.Random.Range(0.0f, 1.0f),
            UnityEngine.Random.Range(0.0f, 1.0f));
    }

    public Color GetRandomSkinColor(Color oldSkinColor)
    {
        var finalColor = oldSkinColor;
        if (this._skinColors.Count > 1)
        {
            while (finalColor == oldSkinColor)
            {
                finalColor = this._skinColors[UnityEngine.Random.Range(0, this._skinColors.Count)];
            }
        }
        return finalColor;
    }

    public Color GetNextSkinColor(Color previousColor)
    {
        Debug.Log("start of GetNextSkinColor" + previousColor);
        var index = this._skinColors.FindIndex(color => (color == previousColor));
        if (index != -1) {
            if ((index + 1) >= this._skinColors.Count)
            {
                return this._skinColors[0];
            }
            else
            {
                Debug.Log("return of GetNextSkinColor" + this._skinColors[index + 1]);
                return this._skinColors[index + 1];
            }
        }

        return this._skinColors[0];
    }

    private void LoadSkinColors()
    {
        // In increasing order of light to dark
        Color currentColor;
        ColorUtility.TryParseHtmlString("#6D570FFF", out currentColor);
        this._skinColors.Add(currentColor);
        ColorUtility.TryParseHtmlString("#967E2FFF", out currentColor);
        this._skinColors.Add(currentColor);
        ColorUtility.TryParseHtmlString("#BA9E40FF", out currentColor);
        this._skinColors.Add(currentColor);
        ColorUtility.TryParseHtmlString("#D2B656FF", out currentColor);
        this._skinColors.Add(currentColor);
        ColorUtility.TryParseHtmlString("#EACE70FF", out currentColor);
        this._skinColors.Add(currentColor);
        ColorUtility.TryParseHtmlString("#FFE89EFF", out currentColor);
        this._skinColors.Add(currentColor);
        ColorUtility.TryParseHtmlString("#FFF4D0FF", out currentColor);
        this._skinColors.Add(currentColor);
    }
}
