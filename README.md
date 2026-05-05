# CatFishing - Trabajo de Fin de Grado (DAM)

> Un videojuego 3D de simulación, recolección y gestión que combina la experiencia cozy con mecánicas profundas de progresión.

[![Estado](https://img.shields.io/badge/Estado-Completado-brightgreen)](https://github.com/inoixh/TFG_2DAM)
[![Motor](https://img.shields.io/badge/Motor-Unity%203D-black?logo=unity)](https://unity.com)
[![Lenguaje](https://img.shields.io/badge/Lenguaje-C%23-239120?logo=csharp)](https://docs.microsoft.com/es-es/dotnet/csharp/)
[![Backend](https://img.shields.io/badge/Backend-Firebase-FFA726?logo=firebase)](https://firebase.google.com)

---

## Tabla de Contenidos

- [Descripción del Proyecto](#descripción-del-proyecto)
- [Características Principales](#características-principales)
- [Tecnologías](#tecnologías)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Estructura de Datos](#estructura-de-datos)
- [Instalación](#instalación)
- [Compilación](#compilación)
- [Testing](#testing)
- [Información Académica](#información-académica)

---

## Descripción del Proyecto

CatFishing es un videojuego de gestión y simulación desarrollado como Trabajo de Fin de Grado para el ciclo de Desarrollo de Aplicaciones Multiplataforma (DAM) en la Universidad Alfonso X el Sabio.

El juego sitúa al jugador en una tranquila isla donde el objetivo principal es pescar diversos peces para crear vínculos emocionales con personajes felinos. A diferencia de otros juegos de gestión convencionales, CatFishing pone un gran énfasis en:

- **Experiencia cozy**: Cada interacción está diseñada para ser relajante y sin estrés
- **Vínculos emocionales**: Los gatos tienen personalidades únicas y reaccionan a tu comportamiento
- **Sostenibilidad ambiental**: El proyecto integra conceptos de reciclaje, crafteo y cuidado del ecosistema

---

## Características Principales

### Minijuego de Pesca Interactivo
Sistema de pesca con mecánicas fluidas donde debes mantener el cursor alineado con los movimientos del pez durante un tiempo determinado para capturarlo.

### Sistema Dinámico de Afinidad
- Cada gato tiene características, ID y rareza propios
- Interactuar y entregar el pez favorito aumenta la amistad progresivamente
- Los gatos cambian de estado emocional en función de la relación
- Sistema de personalidad que influye en el comportamiento

### Colección y Progresión
- Álbum interactivo que registra todos los peces descubiertos
- Sistema de niveles de amistad con cada gato
- Desbloqueo de contenido especial según progresión
- Estadísticas de juego en tiempo real

### Economía de Mercado y Sistema de Misiones
- Tienda interactiva para comprar ítems y contratar servicios
- Servicio de ayudante de pesca para mejorar productividad
- Sistema económico basado en la venta de recursos
- Misiones dinámicas que impulsan la progresión
- Recompensas variadas según dificultad

### Autenticación y Guardado en la Nube
- Autenticación de usuarios integrada con Firebase
- Persistencia de datos totalmente sincronizada
- Hasta 3 slots de guardado por cuenta de usuario
- Sincronización automática entre dispositivos

### Interfaz Intuitiva
- Sistema de detección de proximidad que despliega diálogos contextuales
- Instrucciones en pantalla que aparecen solo cuando es relevante
- Menús intuitivos y responsivos
- Accesibilidad mejorada

---

## Tecnologías

| Componente | Tecnología |
|-----------|-----------|
| Motor Gráfico | Unity 3D |
| Lenguaje Principal | C# |
| Backend / Base de Datos | Firebase (Cloud Firestore & Authentication) |
| Testing | NUnit |
| Control de Versiones | Git / GitHub |

### Arquitectura

El proyecto sigue una arquitectura Data-Driven, donde el contenido del juego se alimenta dinámicamente desde la nube:

```
┌─────────────────────────────────────────────┐
│         Cliente Unity (C#)                   │
│  ├─ Input Manager                           │
│  ├─ UI System                               │
│  ├─ Game Logic                              │
│  └─ Local Cache Manager                     │
└──────────────┬──────────────────────────────┘
               │ Firebase SDK
┌──────────────▼──────────────────────────────┐
│      Firebase Backend Services              │
│  ├─ Cloud Firestore (Base de Datos)        │
│  ├─ Authentication                          │
│  ├─ Cloud Storage                           │
│  └─ Realtime Database (sincronización)     │
└─────────────────────────────────────────────┘
```

---

## Estructura del Proyecto

```
TFG_2DAM/
├── Desarrollo/                          # Documentación y recursos de desarrollo
│   ├── Memoria_TFG_Ainhoa.pdf          # Memoria final del proyecto
│   ├── Propuesta TFG.pdf                # Propuesta inicial
│   ├── Fotos3D/                         # Screenshots de modelos 3D
│   │   ├── Isla.png                    # Visualización de la isla
│   │   ├── Player.png                  # Modelo del jugador
│   │   ├── Peces.png                   # Modelos de peces
│   │   ├── BattleCat.png               # Modelo de gato de batalla
│   │   └── CollidersIsla.png           # Sistema de colisiones
│   └── jsonBD/                          # Base de datos en JSON
│       ├── cats.json                   # Datos de gatos (características, ID, rareza)
│       ├── fish.json                   # Datos de peces (tipos, rareza, ubicación)
│       ├── items.json                  # Objetos del mercado
│       └── services.json               # Servicios disponibles
└── Unity/                               # Proyecto del videojuego
    └── CatFishing/                      # Proyecto principal Unity
        ├── Assets/                      # Recursos del juego
        │   ├── Scripts/                # Código fuente (C#)
        │   ├── Scenes/                 # Escenas del juego
        │   ├── Models/                 # Modelos 3D
        │   ├── Textures/               # Texturas y materiales
        │   ├── Audio/                  # Música y efectos de sonido
        │   ├── Prefabs/                # Prefabs reutilizables
        │   ├── UI/                     # Recursos de interfaz
        │   └── ExternalDependencyManager/  # Gestor de dependencias
        ├── Packages/                    # Dependencias UPM
        │   └── manifest.json            # Especificación de paquetes
        ├── ProjectSettings/             # Configuración del proyecto Unity
        ├── Run/                         # Builds compilados y ejecutables
        ├── .gitignore                  # Configuración de Git
        ├── .vscode/                    # Configuración de VS Code
        └── CatFishing.slnx             # Solución de Visual Studio
```

### Estructura de Scripts

```
Assets/Scripts/
├── Core/                  # Sistemas principales
│   ├── GameManager.cs
│   ├── FirebaseManager.cs
│   └── DataManager.cs
├── Systems/               # Sistemas de juego
│   ├── FishingSystem/
│   ├── CatSystem/
│   ├── MarketSystem/
│   └── QuestSystem/
├── UI/                    # Scripts de interfaz
│   ├── MenuManager.cs
│   ├── UIController.cs
│   └── DialogueSystem.cs
├── Player/                # Scripts del jugador
│   ├── PlayerController.cs
│   ├── PlayerInventory.cs
│   └── PlayerStats.cs
├── NPCs/                  # Scripts de personajes
│   ├── CatBehavior.cs
│   ├── NPCInteraction.cs
│   └── Dialogue.cs
└── Utilities/             # Funciones auxiliares
    ├── InputManager.cs
    ├── AudioManager.cs
    └── SaveManager.cs
```

### Estructura de Datos - Desarrollo

**Documentación:**
- `Memoria_TFG_Ainhoa.pdf` (~1.7 MB) - Documentación técnica completa
- `Propuesta TFG.pdf` (~31 KB) - Propuesta inicial

**Recursos Visuales (Fotos3D/):**
- Capturas de modelos 3D utilizados en el proyecto
- Visualización del mapa principal y elementos del juego

**Base de Datos (jsonBD/):**
- `cats.json` (~134 KB) - Base de datos de gatos con características
- `fish.json` (~12 KB) - Definición de especies de peces
- `items.json` (~6 KB) - Catálogo de objetos del mercado
- `services.json` (~3 KB) - Servicios y ayudantes disponibles

---

## Estructura de Datos

El ecosistema del juego está centralizado en una base de datos Firestore con la siguiente estructura:

### Colecciones Principales

**users** - Gestión de cuentas y progresión
- Autenticación de usuarios
- Ranuras de guardado (save_slots)
- Progresión del juego
- Estadísticas de jugador

**cats** - Especies felinas disponibles
- Datos estáticos de razas
- Características y rareza
- Atributos de personalidad
- Preferencias alimentarias

**fish** - Especies de peces
- Tipos de peces disponibles
- Rareza y ubicación
- Atributos especiales
- Valor en mercado

**items** - Objetos del mercado
- Inventario disponible
- Precios y rareza
- Descripciones
- Efectos especiales

**services** - Servicios y ayudantes
- Ayudante de pesca
- Mejoras de productividad
- Costos asociados
- Efectos en gameplay

**quests** - Sistema de misiones
- Misiones activas
- Requisitos y objetivos
- Recompensas
- Estados de progresión

---

## Instalación

### Requisitos Previos
- Unity 2022.3 LTS o superior
- C# 9.0 o compatible
- Cuenta de Firebase (configurada)
- Git para control de versiones

### Pasos de Configuración

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/inoixh/TFG_2DAM.git
   cd TFG_2DAM/Unity/CatFishing
   ```

2. **Abrir en Unity**
   - Abrir Unity Hub
   - Seleccionar "Abrir proyecto"
   - Navegar a `Unity/CatFishing`
   - Unity descargará las dependencias automáticamente

3. **Configurar Firebase**
   - Crear proyecto en [Firebase Console](https://console.firebase.google.com)
   - Descargar archivo de configuración (google-services.json o GoogleService-Info.plist)
   - Colocar en la carpeta de configuración correspondiente
   - Configurar reglas de seguridad en Firestore

4. **Ejecutar el Proyecto**
   - Abrir escena principal en el editor
   - Hacer click en el botón Play
   - O compilar para tu plataforma objetivo

---

## Compilación

Para compilar el proyecto para diferentes plataformas:

```bash
# Windows
unity -batchmode -nographics -projectPath ./Unity/CatFishing -buildWindowsPlayer ./Build/CatFishing.exe

# macOS
unity -batchmode -nographics -projectPath ./Unity/CatFishing -buildOSXUniversalPlayer ./Build/CatFishing.app

# Android
unity -batchmode -nographics -projectPath ./Unity/CatFishing -buildAndroidPlayer ./Build/CatFishing.apk

# WebGL
unity -batchmode -nographics -projectPath ./Unity/CatFishing -buildWebGL ./Build/WebGL
```

---

## Testing

El proyecto utiliza NUnit para pruebas automatizadas:

```bash
# Ejecutar todas las pruebas
unity -batchmode -runTests -testPlatform editmode

# Ejecutar pruebas específicas
unity -batchmode -runTests -testPlatform editmode -testCategory "Systems"
```

Áreas principales de testing:
- Sistemas de juego (pesca, afinidad, mercado)
- Integración con Firebase (autenticación, sincronización)
- Lógica de progresión (misiones, desbloqueos)
- Interfaz de usuario (flujos de interacción)

---

## Control de Versiones

El proyecto utiliza Git para control de versiones. Ramas principales:

- `main` - Versión estable y lista para producción
- `develop` - Rama de desarrollo
- `feature/*` - Nuevas características
- `bugfix/*` - Correcciones de errores

---

## Información Académica

**Título del Proyecto:** CatFishing - Videojuego de Gestión 3D

**Autor:** Ainhoa Fernández Vadillo

**Tutor:** Carlos Rufiángel

**Ciclo Formativo:** Desarrollo de Aplicaciones Multiplataforma (DAM)

**Centro:** Universidad Alfonso X el Sabio (UAX FP)

**Año Académico:** 2025-2026

**Estado:** Completado

---

## Documentación Adicional

- Memoria Completa: Ver archivo `Desarrollo/Memoria_TFG_Ainhoa.pdf`
- Propuesta del Proyecto: Ver archivo `Desarrollo/Propuesta TFG.pdf`
- Datos del Juego: Revisar archivos JSON en `Desarrollo/jsonBD/`

---

## Contacto

Para preguntas o información adicional sobre el proyecto:

- **Autor:** Ainhoa Fernández Vadillo
- **GitHub:** [@inoixh](https://github.com/inoixh)
- **Institución:** Universidad Alfonso X el Sabio

---

## Licencia

Este proyecto es privado y está protegido bajo licencia académica. No está permitida la distribución o uso comercial sin consentimiento explícito.

---

Última actualización: 2026-05-05
