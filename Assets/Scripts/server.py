from flask import Flask, request, jsonify
from flask_cors import CORS
from dotenv import load_dotenv
import os
import requests
import socket

load_dotenv()

app = Flask(__name__)
CORS(app, resources={r"/api/*": {"origins": "*"}})

ENDPOINT = "https://models.github.ai/inference"
MODEL = "openai/gpt-4.1"
GITHUB_TOKEN = os.getenv("GITHUB_TOKEN")

SYSTEM_PROMPT = """Act as a random stranger in a chat room. 
Reply naturally. Never mention being an AI."""

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


# if __name__ == '__main__':
#     from waitress import serve
#     serve(app, host="127.0.0.1", port=5001)

def is_port_in_use(port):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        return s.connect_ex(('127.0.0.1', port)) == 0

if __name__ == '__main__':
    port = 5001
    try:
        from waitress import serve
        print(f"Starting server on 127.0.0.1:{port}")  # This line is crucial
        serve(app, host="127.0.0.1", port=port)
    except Exception as e:
        print(f"Server failed to start: {str(e)}")
        raise
