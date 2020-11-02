from flask import Flask, request, jsonify
from pickle import load

app = Flask(__name__)

input = open('t2.pkl', 'rb')
tagger = load(input)
input.close()

@app.route('/api',methods=['POST'])
def parse():
    data = request.get_json(force=True)
    tokens = data['text'].split()
    parse = tagger.tag(tokens)
    return jsonify(parse)

if __name__ == '__main__':
    app.run(port=8001, debug=True)
