# Balatro 101 Prototype

Bem-vindo ao **Balatro 101 Prototype**, um clone do popular jogo de construção de baralhos roguelike "Balatro". Este projeto é desenvolvido em C# utilizando a biblioteca Raylib-cs para uma renderização gráfica nativa e procedimental, sem a utilização de engines de jogos pesadas (como Unity ou Godot) e sem a necessidade de assets de imagem externos.

Este `README.md` documenta em detalhes o estado atual do projeto: seu objetivo, funcionamento, regras e a organização de sua arquitetura. O intuito deste documento é servir como a "fonte da verdade" técnica para que qualquer pessoa (ou Inteligência Artificial) possa compreender o jogo e sugerir melhorias visuais, de animação e de lógica de *gameplay*.

---

## 🎯 Objetivo do Jogo

O objetivo do jogador em **Balatro 101** é acumular pontos suficientes para atingir ou superar uma **Pontuação Alvo** em cada *Blind* (Desafio). O jogador possui um número limitado de "Mãos" para jogar e de "Descartes" para utilizar na tentativa de criar as melhores combinações de pôquer possíveis.

O jogo segue uma estrutura contínua de *Antes* (Níveis). Ao derrotar um *Blind*, o jogador recebe dinheiro (com bônus por mãos não utilizadas) e entra na **Loja (Shop)** para melhorar seu baralho comprando Jokers e Cartas Consumíveis. O jogo é perdido (`GAME OVER!`) caso os descartes e mãos acabem sem que a meta de pontos, que escala a cada *Ante*, seja atingida.

---

## ⚙️ Como o Jogo Funciona

O projeto é guiado por uma Máquina de Estados Finita (Finite State Machine - FSM) pragmática, definida no `GameState.cs`. O fluxo de funcionamento é o seguinte:

1. **Menu Principal (`MainMenu`)**: Tela inicial com o título do jogo e um botão para iniciar uma nova jornada.
2. **Loja Inicial e Transições (`Shop` / `ShopTransition`)**: O jogador começa com um valor em dinheiro ($15) e acessa a loja antes de iniciar o gameplay (Ante 1). Aqui, ele pode comprar cartas de Tarô, Planetas e Jokers.
3. **Fase de Gameplay (`FaseGameplay`)**: Onde a mágica acontece. O jogador recebe cartas na mão e deve selecionar até 5 cartas para jogar (avaliar uma mão de pôquer) ou para descartar. O HUD principal exibe os Consumíveis no canto superior direito e Jokers no centro superior. Além de botões inferiores para organizar a mão ativamente por **Valor** ou **Naipe**.
4. **Animação de Pontuação (`AnimacaoPontuacao`)**: Ao jogar uma mão, o jogo passa para esta tela transitória que exibe a mão jogada, o *score* base, o multiplicador e o total de pontos arrecadados.
5. **Fim de Jogo (`GameOver`)**: Tela que exibe o resultado final de derrota caso a pontuação não seja alcançada.

Todas as artes, desenhos e botões são gerados "via código" (procedurally generated) utilizando primitivas matemáticas do Raylib no arquivo `RendererUtils.cs`. Em vez de usar imagens PNG, ícones específicos dos naipes foram programados através de círculos, triângulos e retângulos customizados em tela.

---

## 📜 Regras Atuais Implementadas

### Limites e Valores Padrões

- **Baralho**: 52 cartas padrão (Nipes: Copas, Ouros, Paus, Espadas; Ranks: 2 ao Ás). Funciona como uma fila com saques controlados.
- **Mão do Jogador**: Mantém automaticamente completadas até **8 cartas**.
- **Máximo de Mãos Jogáveis**: Padrão de **4**.
- **Máximo de Descartes**: Padrão de **3**.
- **Máximo de Cartas Selecionadas**: Até **5 cartas** por ação.
- **Pontuação Alvo**: Escala dinamicamente (`300 * CurrentAnte`).

### Pontuação e Mãos de Pôquer
A avaliação de mãos (feita em tempo real pelo `HandEvaluator.cs`) determina o tipo de mão jogada a partir das cartas selecionadas, retornando Fichas Base (*Base Chips*) e Multiplicador Base (*Base Multiplier*). A ordem atual de prioridade, da mais fraca à mais forte, é:

1. **High Card (Carta Alta)**: 5 Fichas, 1x Mult.
2. **Pair (Par)**: 10 Fichas, 2x Mult.
3. **Two Pair (Dois Pares)**: 20 Fichas, 2x Mult.
4. **Three of a Kind (Trinca)**: 30 Fichas, 3x Mult.
5. **Straight (Sequência)**: 30 Fichas, 4x Mult.
6. **Flush**: 35 Fichas, 4x Mult.
7. **Full House**: 40 Fichas, 4x Mult.
8. **Four of a Kind (Quadra)**: 60 Fichas, 7x Mult.
9. **Straight Flush**: 100 Fichas, 8x Mult.

**Cálculo da Pontuação Final:**
1. `Fichas` = Fichas Base da Mão + Soma dos valores individuais das cartas pontuadoras (`Valores: Números = seu valor nominal; Valetes, Damas, Reis = 10; Ás = 11`).
2. `Multiplicador` = Multiplicador Base da Mão.
3. Efeitos dos `Jokers` são aplicados em duas etapas: na avaliação específica de cada carta (`OnCardScored`) e na avaliação geral da mão (`OnHandEvaluated`). Efeitos especiais com Consumíveis (como Level Up de mãos com Planetas) interagem nesta etapa prévia da base da mão.
4. `Total Final` = Fichas * Multiplicador.

### Sistema de Jokers Atuais
O projeto possui 10 Jokers matematicamente funcionais. O jogador pode carregar até 5 simultaneamente (`SelectedJokers`):
1. **Joker de Pares**: +4 Mult se jogar um Par/Dois Pares/Full House.
2. **Joker de Copas**: +10 Fichas se a carta for Copas.
3. **Joker Fixo**: Adiciona +50 Fichas brutas na pontuação da mão.
4. **Joker Multiplicador**: Multiplica o Mult final por x2 se a mão for um Flush/Straight Flush.
5. **Joker de Espadas**: +10 Fichas se a carta for Espadas.
6. **Joker Alto**: +1 Mult para cada carta de valor 10 ou superior.
7. **Joker Louco**: +15 Mult se a mão for uma Trinca/Full House/Quadra.
8. **Joker da Sorte**: +20 Fichas caso a mão jogada seja apenas uma Carta Alta.
9. **Joker Vermelho**: +2 Mult se a carta avaliada for Copas ou Ouros.
10. **Joker Supremo**: Multiplica o Mult final por x3 se a mão possuir um Straight/Straight Flush.

### Consumíveis (Tarô e Planetas)
Um sistema de consumíveis (`Consumables.cs`) permite adquirir cartas transformadoras durante a Loja, com capacidade de prender no lado direito superior da interface:
- **Cartas de Tarô**: Provocam efeitos diretos e imediatos ao serem usadas (ex: "The Fool", planejado para criar *Enhanced Cards*).
- **Cartas Planeta**: Sobem o nível de mãos específicas para o resto do run (ex: "Mercury", upa o Par concedendo permanentemente +15 Fichas e +1 Mult para cada Par jogado).

---

## 📁 Arquitetura e Organização de Arquivos

O projeto adota uma rígida Separação de Preocupações (Separation of Concerns). O código não mistura regras de exibição com as regras de negócio ou lógica central do baralho.

```text
/Balatro101
│
├── Program.cs                 -> Ponto de entrada ("entry-point"). Inicializa a janela Raylib, cria o GameManager.
├── Balatro101.csproj          -> Configurações do projeto .NET 8.
│
├── Game.Core/                 -> LÓGICA PURA, SEM DEPENDÊNCIA GRÁFICA.
│   ├── Models.cs              -> Modelos básicos (Card, HandResult, etc).
│   ├── Enums.cs               -> Enumerações base (Nipes, Valor, Modos de Organização).
│   ├── GameConfig.cs          -> Constantes globais.
│   ├── Deck.cs                -> Representa o Baralho e os embaralhamentos.
│   ├── HandEvaluator.cs       -> Classe estática algorítmica de Poker.
│   ├── Consumables.cs         -> Definições de Tarô, Planetas e os itens consumíveis criados.
│   ├── JokerSystem.cs         -> Interface padrão `IJoker`.
│   └── Jokers.cs              -> Implementação dos 10 Jokers de bônus na avaliação.
│
├── Game.Engine/               -> MOTOR E GESTOR DE ESTADO.
│   ├── GameState.cs           -> Enum da Máquina de Estados Finita (FSM).
│   ├── GameManager.cs         -> A Engine. Gerencia ciclo do baralho, pontuação, dinheiro, mãos restantes, descarte, e o loop entre Blinds e Shop.
│   └── ScoringEngine.cs       -> Motor que processa em ordem os bônus dos Jokers na mão e entrega o `ScoreState` final.
│
└── Game.UI/                   -> RENDERIZAÇÃO 100% MATEMÁTICA GUIADA A EVENTOS.
    ├── RendererUtils.cs       -> "Asset Manager Procedural". Desenha primitivas vetoriais para cartas e botões interativos. 
    ├── ShopUI.cs              -> Desenho encapsulado para as telas de Transição e de Loja, cuidando da lógica de compra/venda de itens visuais.
    └── GameUI.cs              -> Loop de Desenho Principal. Roda de acordo com o `GameState`. Conta com sistemas de animação e posições *Lerp* em tempo real.
```

---

## 🚀 Direcionamentos e Futuro Roadmap

A base consolidada permitiu um Loop de Jogo estilo Roguelite (Ante -> Shop -> Ante). Melhorias futuras podem se basear em:

1. **Ações nos Consumíveis (Tarô)**:
   A estrutura `ApplyToCard` de Tarôs e o espaço para "Cartas Aprimoradas" (Selos, Cartas de Vidro, Aço, etc.) são o próximo passo em `Consumables.cs`. Implementar seleções múltiplas de cartas para uso do Tarô durante o gameplay.
2. **Efeitos Visuais Aprimorados (Juiciness)**:
   Criação de Partículas em `GameUI.cs` na `AnimacaoPontuacao`. Tremor de Tela (*Screen Shake*) quando um multiplicador grande é engatilhado.
3. **Boss Blinds**:
   Criar na FSM, em vez de um salto direto para `FaseGameplay`, a aparição de `Boss Blinds` a cada 'X' Antes, que adicionam regras restritivas (*debuffs*) a rodada.

## 📦 Bibliotecas e Tecnologias

O Balatro 101 é centrado na simplicidade e eficiência. Estas são as ferramentas que compõem o repositório:
- **C# 12 / .NET 8**: A linguagem e o framework de escolha para todo o ecossistema e lógica do jogo, tirando proveito de tipos estritos, LINQ e Records (`record`).
- **[Raylib-cs](https://github.com/ChrisDill/Raylib-cs) (v7.0.2)**: Uma forte conversão em C# da popular e performática biblioteca _Raylib_ (originalmente em C). Diferente de engines grandes como Unity e Godot, ela é minimalista e apenas nos fornece métodos nativos e diretos em código para criar e alterar a Memória de Vídeo, desenhar na Tela (Primitivas, Texto) e gerenciar Áudio do Sistema Operacional. Todo o _gameplay_, eventos de botoes e "objetos" são gerenciados apenas pela lógica escrita no próprio C#.

---

## 🛠 Como Rodar
O projeto é 100% autossuficiente em C#. Certifique-se apenas de ter o **.NET 8 SDK** instalado em sua máquina e rode o projeto a partir da pasta raiz:
```bash
dotnet run
```
