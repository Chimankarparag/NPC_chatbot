# NPC Chatbot for Unity

An enhanced Unity-based NPC chatbot system inspired by [srcnalt/OpenAI-Unity](https://github.com/srcnalt/OpenAI-Unity), 
 ### Since it is a built-on, please Download the package by srcnalt (Sarge) and import OpenAI before proceeding
 
 Worked on:
- **Updated GitHub API Integration**: Seamless communication with GitHub's API.
- **Local Flask Server for API Calls**: Efficient handling of API requests via a local server.
- **User-Friendly NPC Interaction**: Intuitive UI components for engaging NPC dialogues.


![NPC2](https://github.com/user-attachments/assets/dd9db617-ef3d-4b35-b4c9-7a2f26ba59f9)


## Features

- **GitHub API Integration**: Utilize GitHub's API for dynamic data retrieval.
- **Local Flask Server**: Handle API requests locally to reduce latency and improve performance.
- **Interactive NPC Dialogues**: Engage with NPCs through a responsive chat interface.
- **Modular Script Architecture**: Organized scripts for maintainability and scalability.

## Project Structure

```
Assets/
├── .env                     # Environment variables (e.g., API keys)
├── Scripts/
│   ├── GithubApi.cs         # Handles GitHub API interactions
│   ├── ServerRunner.cs      # Manages the local Flask server
│   ├── CallFromLocal.cs     # Facilitates communication with the Flask server
│   └── NPCBehaviour.cs      # Defines NPC behaviors and interactions
├── Prefabs/
│   ├── SendPrefab.prefab    # UI prefab for sent messages
│   └── ReceivePrefab.prefab # UI prefab for received messages
```

## Setup Instructions

### Prerequisites

#### GithubApi Key : 
Github Marketplace -> (preferable and tested) OpenAI -> Use this Model -> Generate Developer Token -> classic -> copy and paste to .env file


- Unity 2020.3 or later
- Python 3.7 or later
- Flask (`pip install flask`)
- GitHub Personal Access Token (for API access)

### Unity Configuration

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/Chimankarparag/NPC_chatbot.git
   ```

2. **Open in Unity**:
   - Launch Unity Hub.
   - Click on "Add" and select the cloned project folder.

3. **Set Up Environment Variables**:
   - Create a `.env` file inside the `Assets` directory.
   - Add your GitHub Personal Access Token:
     ```env
     GITHUB_TOKEN=your_personal_access_token
     ```

4. **Configure UI Elements**:

   - Ensure the following UI components are present in your scene:
     - `InputField`: For user text input.
     - `SendButton`: Triggers message sending.
     - `ScrollView`: Displays the chat history.
     - `SendPrefab`: Prefab for displaying sent messages.
     - `ReceivePrefab`: Prefab for displaying received messages.
       
![NPC1_ediotr](https://github.com/user-attachments/assets/bddc5d5a-1953-41b8-b12b-bb0d4de969d2)

### Flask Server Setup

1. **Navigate to the Server Directory**:
   ```bash
   cd NPC_chatbot/FlaskServer
   ```

2. **Install Dependencies**:
   ```bash
   pip install -r requirements.txt
   ```

3. **Run the Server**:
   ```bash
   python app.py
   ```

   The server will start on `http://localhost:5000`.

### Running the Application

- Press the Play button in Unity.
- Interact with NPCs through the chat interface.
- Messages will be processed via the local Flask server and GitHub API.

## Scripts Overview

- **GithubApi.cs**: Initializes and manages GitHub API calls, setting NPC behaviors at the start of gameplay.
- **ServerRunner.cs**: Starts and monitors the local Flask server.
- **CallFromLocal.cs**: Sends and receives data between Unity and the Flask server.
- **NPCBehaviour.cs**: Defines how NPCs respond to user inputs.

## UI Components

Ensure the following prefabs and UI elements are correctly linked:

- **InputField**: `UI/InputField`
- **SendButton**: `UI/SendButton`
- **ScrollView**: `UI/ScrollView`
- **SendPrefab**: `Prefabs/SendPrefab.prefab`
- **ReceivePrefab**: `Prefabs/ReceivePrefab.prefab`


