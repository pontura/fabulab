using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PalettesManager : MonoBehaviour
{
    public List<ColorsData> colors;
    public List<PaletteData> palettes;


    public Sprite arm;
    public Sprite leg;
    public Sprite eyebrows;

    public colorNames[] arms; 
    public colorNames[] legs;
    public colorNames[] eyebrow;

    [Serializable]
    public class ColorsData
    {
        public colorNames name;
        public Color color;
    }
    public enum colorNames
    {
        VERDE,
		VERDE2,
        AMARILLO,
		AMARILLO2,
        ROJO,
        ROSA,
		FUCSIA,
        NARANJA,
        AZUL,
        BLANCO,
        NEGRO,
        CELESTE,
		BEIGE,
		MARRON,
		MARRON2,
		VIOLETA,
		GRIS,
        BLANCO_NEGRO,
		NEGRO_BLANCO,
		AMARILLO_NEGRO,
		NEGRO_AMARILLO,
		CELESTE_NEGRO,
		NEGRO_CELESTE,
		ROSA_NEGRO,
		NEGRO_ROSA,
		ROSA_VIOLETA,
		VIOLETA_ROSA,
		MARRON_MARRON2,
		MARRON2_MARRON,
		ROJO_VERDE,
		VERDE_ROJO,
		ROJO_AMARILLO,
		AMARILLO_ROJO,
		ROJO_BLANCO,
		BLANCO_ROJO,
		VIOLETA_BLANCO,
		BLANCO_VIOLETA,
		ROSA_NARANJA,
		NARANJA_ROSA,
		VERDE_VERDE2,
		VERDE2_VERDE,
		ROSA_AMARILLO,
		AMARILLO_ROSA,
		BLANCO_AZUL,
		AZUL_BLANCO,
		VERDE_AMARILLO,
		AMARILLO_VERDE,
		NARANJA_AMARILLO,
		AMARILLO_NARANJA,
		CELESTE_AMARILLO,
		AMARILLO_CELESTE,
		ROSA_BLANCO,
		BLANCO_ROSA,
		CELESTE_BLANCO,
		BLANCO_CELESTE,
		BEIGE_MARRON,
		MARRON_BEIGE
		
       
    }
    public enum paletteNames
    {
        BASICA,
		NATURALISTA,
		ACUATICA,
		PUMPUM,
		CANDY,
		BASICA_2,
		NATURALISTA_2,
		ACUATICA_2,
		PUMPUM_2,
		CANDY_2
    }
    [Serializable]
    public class PaletteData
    {
        public paletteNames paletteName;
        public colorNames[] colors;
    }
    public PaletteData GetPaletteData(paletteNames name)
    {
        foreach (PaletteData data in palettes)
            if (data.paletteName == name)
                return data;
        return null;
    }
    public int GetId(paletteNames paletteName, Color _color)
    {
        PaletteData pd = palettes.Find(x => x.paletteName == paletteName);

        //for(int i = 0; i < pd.colors.Length; i++) 
        //{            
        //    if (GetColorByName(pd.colors[i]) == _color)
        //            return i;
        
        //}
        return 0;
    }
    public List<Color> GetColor(paletteNames paletteName, int id)
    {
        foreach(PaletteData paletteData in palettes)
        {
            if (paletteData.paletteName == paletteName)
                return GetColorsByName(paletteData.colors[id]);
        }
        return new List<Color>();         
    }
   
    public List<Color> GetColorsByName(colorNames name)
    {
        List<Color> arr = new List<Color>();
        Color c = GetColor(name);
        if (c != Color.grey)
        {
            arr.Add(c);
            return arr;
        }

        string[] ccc = name.ToString().Split("_"[0]);
        foreach(string n in ccc)
        {
          //  print(n);
            arr.Add(GetColor(n));
        }
        //print(arr.Count);
        return arr;
    }

    public colorNames GetColorName(Color c) {
        return colors.Find(x => x.color == c).name;
    }

    public Color GetColor(colorNames name)
    {
        foreach (ColorsData cData in colors)
            if (name == cData.name)
                return cData.color;
        return Color.grey;
    }
    Color GetColor(string name)
    {
        List<Color> arr = new List<Color>();
        foreach (ColorsData cData in colors)
            if (name == cData.name.ToString())
                return cData.color;
        return Color.grey;
    }
}
