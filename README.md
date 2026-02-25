# 🎣 CatFishing - Trabajo de Fin de Grado (DAM)

**CatFishing** es un videojuego 3D de simulación, recolección y gestión desarrollado como Trabajo de Fin de Grado (TFG) para el ciclo de Desarrollo de Aplicaciones Multiplataforma (DAM) en la Universidad Alfonso X el Sabio (UAX FP). 

Busca ofrecer una experiencia *cozy* (tranquila y libre de estrés), combinando el farmeo relajante con la creación de vínculos emocionales con los personajes del juego. 

## 📖 Sobre el proyecto

El juego sitúa al jugador en una isla donde el objetivo principal es pescar diversos peces para distintos tipos de gatos. A diferencia de otros juegos de gestión, *CatFishing* pone un gran peso en la **afinidad y el cuidado de los personajes**. Cada gato tiene un pez favorito; si se lo proporcionas, vuestra amistad aumentará, desbloqueando nuevas interacciones y reacciones. 

Además de la vertiente lúdica, el proyecto plantea integrar a futuro conceptos de sostenibilidad, enseñando al jugador a reciclar, craftear recursos y cuidar el medio ambiente de la isla.

## ✨ Características principales

* **🎣 Minijuego de Pesca:** Un sistema interactivo donde el jugador debe mantener el cursor alineado con los movimientos del pez durante un tiempo determinado para capturarlo.
* **🐾 Sistema de Afinidad:** Cada gato generado en la isla tiene características e ID propios. Interactuar con ellos y darles su pez favorito aumenta la amistad y cambia su estado de ánimo.
* **📖 Colección y Progresión:** Un álbum interactivo donde se registran todos los peces descubiertos y el nivel de amistad con cada gato.
* **🏪 Mercado interactivo y Misiones:** Un sistema económico donde comprar ítems o servicios (como un ayudante de pesca) y vender recursos, junto con un sistema de misiones que impulsa la evolución del jugador.
* **☁️ Guardado en la Nube (BaaS):** Autenticación de usuarios y persistencia de datos (hasta 3 slots de guardado por cuenta) totalmente integrados con Firebase.
* **💬 Interfaz Intuitiva (UI):** Sistema de detección de proximidad que despliega diálogos e instrucciones en pantalla ("Pulsa espacio") solo cuando estás cerca de un objeto o NPC interactuable.

## 🛠️ Tecnologías y Arquitectura

Este proyecto sigue una arquitectura **Data-Driven** (Orientada a Datos), donde el contenido del juego se alimenta dinámicamente desde la nube.

* **Motor Gráfico:** Unity 3D
* **Lenguaje:** C#
* **Backend / Base de Datos:** Firebase (Cloud Firestore & Authentication).
* **Testing:** NUnit para pruebas automatizadas.

## 🗄️ Estructura de Datos (Firestore)

El ecosistema del juego se centraliza en la base de datos:
* `users`: Gestión de cuentas y ranuras de guardado (`save_slots`).
* `cats` & `fish`: Datos estáticos de las especies y rarezas.
* `items` & `services`: Economía del mercado y ayudantes.
* `quests`: Misiones activas y requisitos.

## 👩‍💻 Autoría

Desarrollado por **Ainhoa Fernández Vadillo**.
* **Tutor:** Carlos Rufiángel
* **Institución:** UAX FP - Universidad Alfonso X el Sabio
