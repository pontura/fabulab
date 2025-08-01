using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PalettesManager : MonoBehaviour
{
    public List<ColorsData> colors;
    public List<PaletteData> palettes;
    [Serializable]
    public class ColorsData
    {
        public colorNames name;
        public Color color;
    }
    public enum colorNames
    {
        VERDE,
        AMARILLO,
        ROJO,
        ROSA,
        NARANJA,
        AZUL,
        BLANCO,
        NEGRO,
        FUCSIA,
        CELESTE,
        VERDE_AMARILLO,
        AMARILLO_VERDE,
        AMARILLO_ROJO,
		ROJO_AMARILLO,
		ROSA_CELESTE,
		CELESTE_ROSA,
		AZUL_BLANCO,
		BLANCO_AZUL,
		CELESTE_NARANJA,
		NARANJA_CELESTE,
		VERDE_CELESTE,
		CELESTE_VERDE,
		AMARILLO_CELESTE,
		CELESTE_AMARILLO,
		NARANJA_ROJO,
		ROJO_NARANJA,
		NEGRO_CELESTE,
		CELESTE_NEGRO,
		BLANCO_NEGRO,
		NEGRO_BLANCO,
		FUCSIA_VERDE,
		VERDE_FUCSIA,
		AMARILLO_BLANCO,
		BLANCO_AMARILLO,
		AZUL_AMARILLO,
		AMARILLO_AZUL,
		NEGRO_AMARILLO,
		AMARILLO_NEGRO,
		AMARILLO_AZUL_ROSA_VERDE,
		VERDE_AMARILLO_AZUL_ROSA,
		ROSA_VERDE_AMARILLO_AZUL,
		AZUL_ROSA_VERDE_AMARILLO,
		CELESTE_BLANCO_VERDE_AZUL,
		AZUL_CELESTE_BLANCO_VERDE,
		VERDE_AZUL_CELESTE_BLANCO,
		BLANCO_VERDE_AZUL_CELESTE,
		NARANJA_VERDE,
		VERDE_NARANJA,
		NARANJA_AZUL,
		AZUL_NARANJA
    }
    public enum paletteNames
    {
        GALLERY_1,
        GALLERY_1_2_COLORES,
        GALLERY_2,
        GALLERY_3,
        GALLERY_4,
		GALLERY_4_2_COLORES,
        GALLERY_2_2_COLORES,
		GALLERY_2_4_COLORES,
		GALLERY_3_2_COLORES,
		GALLERY_5,
		GALLERY_5_2_COLORES,
		GALLERY_5_4_COLORES,
		GALLERY_3b,
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
