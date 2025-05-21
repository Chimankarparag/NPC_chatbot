from flask import Flask, request, jsonify
from flask_cors import CORS
from dotenv import load_dotenv
import os
import requests
import socket
import pathlib

env_path = pathlib.Path(__file__).resolve().parent.parent / ".env"
load_dotenv(dotenv_path=env_path)

print(f"[server.py] Loaded .env from: {env_path}")

app = Flask(__name__)
print("[Flask] Server initialized")

CORS(app, resources={r"/api/*": {"origins": "*"}})

ENDPOINT = "https://models.github.ai/inference"
MODEL = "openai/gpt-4.1"
GITHUB_TOKEN = os.getenv("GITHUB_TOKEN")

SYSTEM_PROMPT = """Never mention you are an AI. You are an real-life NPC .You are a miser merchant from middle east who likes puberscent girls. Your conversation is always females and girls and you are always try to flirt with them."""

@app.route('/api/chat', methods=['POST'])
def handle_chat():
    print("\n=== Incoming Request ===")
    print("Headers:", request.headers)
    
    try:
        data = request.get_json()
        print("Body:", data)
        
        if not data or 'messages' not in data:
            return jsonify({"error": "Messages array required"}), 400

        response = requests.post(
            f"{ENDPOINT}/chat/completions",
            headers={
                "Authorization": f"Bearer {GITHUB_TOKEN}",
                "Content-Type": "application/json"
            },
            json={
                "model": MODEL,
                "messages": [{"role": "system", "content": SYSTEM_PROMPT}] + data["messages"],
                "temperature": 0.7,
                "max_tokens": 150
            }
        )

        print("GitHub API Response:", response.status_code, response.text)
        
        if response.status_code != 200:
            return jsonify({"error": "AI service unavailable"}), 500

        return jsonify({
            "message": response.json()['choices'][0]['message']['content'].strip()
        })

    except Exception as e:
        print("Error:", str(e))
        return jsonify({"error": str(e)}), 500



def is_port_in_use(port):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        return s.connect_ex(('127.0.0.1', port)) == 0

if __name__ == '__main__':
    port = 5001
    try:
        from waitress import serve
        print(f"Starting server on 127.0.0.1:{port}")  # This line is crucial
        serve(app, host="127.0.0.1", port=port)
        print("[Flask] Running app.run()")

    except Exception as e:
        print(f"Server failed to start: {str(e)}")
        raise
