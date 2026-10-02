namespace Assets.PolyRef
{    
    using UnityEngine;
    using System;

    // Menandakan bahwa atribut ini hanya bisa dipakai di sebuah field (variabel)
    [AttributeUsage(AttributeTargets.Field)]
    public class SubclassSelectorAttribute : PropertyAttribute 
    {
        // Kosong saja, ini hanya bertindak sebagai "Label/Stiker"
    }
}