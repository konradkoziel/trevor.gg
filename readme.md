# 🧠 TrevorBot — Discord's Operational Diagnostician

TrevorBot is a Discord bot built on **Azure Functions** and **Azure OpenAI**, designed for teams that thrive on chaos, rituals, and judgmental automation.  
He doesn’t just respond — he observes, analyzes, and occasionally helps.

---

## 🚀 Features

### `/ping`
Checks if Trevor is alive.  
He usually is. Still cynical.

### `/teams gracze: Konrad,Janusz,Jan,Bartosz`
Splits the provided list of players into two teams.  
If they’re uneven — Trevor sees all. And judges.

### `/ask question: <text>`
Connects directly to **Azure OpenAI (GPT-4o)**.  
Trevor reads your question, consults the neural gods, and answers briefly and precisely.  
He’s helpful by contract, but not necessarily polite.

### `/rank nick: Konrad#EUNE`
Pulls Solo/Duo and Flex rank from the **Riot API**.
Trevor reads your LP and tells you exactly what he thinks of it.

---

## 🧰 Tech Stack

- **Azure Functions (.NET 10)** — webhook-based backend for Discord Interactions  
- **Azure OpenAI Service** — GPT-4o model powering `/ask`  
- **Discord Interactions API** — Slash commands with signature verification  
- **C#** — clean structure with DI, async/await, and factory-based command registry  
- **Trevor Personality Engine™** — sarcasm-driven orchestration layer

---
