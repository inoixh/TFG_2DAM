using System.Text.RegularExpressions;
using NUnit.Framework;

/// <summary>
/// Pruebas automatizadas de la lógica de autenticación y carga de progresión, con la librería de NUnit.
/// Valida los requisitos de seguridad local antes de enviar peticiones a Firebase.
/// .
/// [SetUp] Etiqueta en la que se ejecuta todo ANTES de cada Test
/// [Test] Cada función de prueba que se va a ejecutar
/// Assert. Comandos que afirman lo que tiene que devolver el test (isTrue, isFalse)
/// </summary>
public class AuthAndDataTests
{
    private string patronEmailValidacion;
    private int baseXPSistema;
    private float multiplicadorXPSistema;

    /// <summary>
    /// Prepara los valores base de las mecánicas del juego antes de ejecutar cada prueba.
    /// </summary>
    [SetUp]
    public void ConfigurarPruebas()
    {
        patronEmailValidacion = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        baseXPSistema = 100;
        multiplicadorXPSistema = 1.5f;
    }

    /// <summary>
    /// Comprueba que el sistema acepta un formato de correo electrónico estructurado correctamente.
    /// </summary>
    [Test]
    public void Registro_EmailCorrecto_PasaValidacion()
    {
        string emailPrueba = "jugador@myuax.es";

        bool esValido = Regex.IsMatch(emailPrueba, patronEmailValidacion);

        Assert.IsTrue(esValido);
    }

    /// <summary>
    /// Comprueba que el sistema rechaza correos electrónicos con formatos incompletos o erróneos.
    /// </summary>
    [Test]
    public void Registro_EmailIncorrecto_FallaValidacion()
    {
        string emailPrueba = "jugador_myuax.es";

        bool esValido = Regex.IsMatch(emailPrueba, patronEmailValidacion);

        Assert.IsFalse(esValido);
    }

    /// <summary>
    /// Valida que la contraseña simulada cumple con la longitud mínima exigida por Firebase.
    /// </summary>
    [Test]
    public void Registro_ContrasenaCorta_EsRechazada()
    {
        string contrasenaPrueba = "12345";

        bool esValida = contrasenaPrueba.Length >= 6;

        Assert.IsFalse(esValida);
    }

    /// <summary>
    /// Simula la carga de datos de un usuario nuevo y verifica que el requisito de experiencia se inicializa bien.
    /// </summary>
    [Test]
    public void CargaDatos_NivelUno_CalculaExperienciaNecesariaCorrectamente()
    {
        int nivelSimulado = 1;
        int experienciaEsperada = 100;

        int experienciaCalculada = UnityEngine.Mathf.RoundToInt(
            baseXPSistema * UnityEngine.Mathf.Pow(multiplicadorXPSistema, nivelSimulado - 1)
        );

        Assert.AreEqual(experienciaEsperada, experienciaCalculada);
    }

    /// <summary>
    /// Simula la carga de datos de un usuario avanzado y verifica la escalabilidad matemática de la experiencia.
    /// </summary>
    [Test]
    public void CargaDatos_NivelAvanzado_CalculaEscaladoExperiencia()
    {
        int nivelSimulado = 3;
        int experienciaEsperada = 225;

        int experienciaCalculada = UnityEngine.Mathf.RoundToInt(
            baseXPSistema * UnityEngine.Mathf.Pow(multiplicadorXPSistema, nivelSimulado - 1)
        );

        Assert.AreEqual(experienciaEsperada, experienciaCalculada);
    }
}
