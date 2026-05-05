using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestor global de la interfaz de usuario.
/// Controla el panel de diálogos, barras de progresión de afinidad y los textos de alertas.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Sistema de Diálogos")]
    public GameObject panelDialogo;
    public TextMeshProUGUI textoDialogo;

    [Header("Progresión Afinidad (En Diálogo)")]
    public GameObject panelBarraAfinidad;
    public Slider barraAfinidad;
    public TextMeshProUGUI textoNivelAfinidad;

    [Header("Textos Directos")]
    public TextMeshProUGUI textoInteraccion;
    public TextMeshProUGUI textoTooltip;

    void Awake() 
    { 
        Instance = this; 
        
        if (panelDialogo) panelDialogo.SetActive(false);
        if (panelBarraAfinidad) panelBarraAfinidad.SetActive(false);
        if (textoInteraccion) textoInteraccion.gameObject.SetActive(false);
        if (textoTooltip) textoTooltip.gameObject.SetActive(false);
    }

    public void MostrarBocadillo(string frase) 
    { 
        if (panelDialogo) panelDialogo.SetActive(true); 
        if (textoDialogo) textoDialogo.text = frase; 
    }
    
    public void OcultarBocadillo() 
    { 
        if (panelDialogo) panelDialogo.SetActive(false); 
        if (panelBarraAfinidad) panelBarraAfinidad.SetActive(false);
    }

    /// <summary>
    /// Actualiza y muestra la barra de progreso de amistad con el gato actual.
    /// </summary>
    public void MostrarAfinidadGato(int nivel, int xpActual, int xpNecesaria)
    {
        if (panelBarraAfinidad) panelBarraAfinidad.SetActive(true);
        if (textoNivelAfinidad) textoNivelAfinidad.text = $"Afinidad Lvl {nivel} | XP {xpActual}/{xpNecesaria}";
        if (barraAfinidad)
        {
            barraAfinidad.maxValue = xpNecesaria;
            barraAfinidad.value = xpActual;
        }
    }

    public void MostrarInteraccion(string mensaje) 
    { 
        if (textoInteraccion) 
        {
            textoInteraccion.gameObject.SetActive(true); 
            textoInteraccion.text = mensaje; 
        }
    }
    
    public void OcultarInteraccion() { if (textoInteraccion) textoInteraccion.gameObject.SetActive(false); }

    public void MostrarTooltip(string mensaje) 
    {
        if (textoTooltip) 
        {
            CancelInvoke("OcultarTooltip");
            textoTooltip.gameObject.SetActive(true);
            textoTooltip.text = mensaje;
        }
    }

    public void MostrarTooltipTemporal(string mensaje, float tiempo) 
    {
        MostrarTooltip(mensaje);
        Invoke("OcultarTooltip", tiempo);
    }

    public void OcultarTooltip() { if (textoTooltip) textoTooltip.gameObject.SetActive(false); }

    public void MostrarSubidaNivelGlobal(int nivel)
    {
        MostrarTooltipTemporal($"¡NIVEL AUMENTADO A {nivel}!", 4f);
    }
}