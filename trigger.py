import nltk,math
import json
import sklearn
from sklearn import metrics
from nltk.tag import brill, brill_trainer 
import csv
import random

with open('./tagger_training_data.json') as f:
    training_sents = json.load(f)

training_sents = [[ tuple(j) for j in i ] for i in training_sents]
random.shuffle(training_sents)

size = int(len(training_sents) * 0.7)
train_sents = training_sents[:size]
test_sents = training_sents[size:]

#PerceptronTagger
tagged_sentences = [sentence for sentence in training_sents]
random.shuffle(tagged_sentences)
split_idx = math.floor(len(tagged_sentences)*0.3)
test_sentences = tagged_sentences[0:split_idx]
train_sentences = tagged_sentences[split_idx:]

perceptron_tagger = nltk.tag.perceptron.PerceptronTagger(load=False)

# # This will only work if the functions in perceptron.py is modified to not throw an error on empty strings inputs
perceptron_tagger.train(train_sentences)

#Brill Tagger
#Templates acuqired at https://www.geeksforgeeks.org/nlp-brill-tagger/
def train_brill_tagger(initial_tagger, train_sents, **kwargs): 
    templates = [ 
            brill.Template(brill.Pos([-1])), 
            brill.Template(brill.Pos([1])), 
            brill.Template(brill.Pos([-2])), 
            brill.Template(brill.Pos([2])), 
            brill.Template(brill.Pos([-2, -1])), 
            brill.Template(brill.Pos([1, 2])), 
            brill.Template(brill.Pos([-3, -2, -1])), 
            brill.Template(brill.Pos([1, 2, 3])), 
            brill.Template(brill.Pos([-1]), brill.Pos([1])), 
            brill.Template(brill.Word([-1])), 
            brill.Template(brill.Word([1])), 
            brill.Template(brill.Word([-2])), 
            brill.Template(brill.Word([2])), 
            brill.Template(brill.Word([-2, -1])), 
            brill.Template(brill.Word([1, 2])), 
            brill.Template(brill.Word([-3, -2, -1])), 
            brill.Template(brill.Word([1, 2, 3])), 
            brill.Template(brill.Word([-1]), brill.Word([1])), 
            ] 
    # Using BrillTaggerTrainer to train  
    trainer = brill_trainer.BrillTaggerTrainer( 
            initial_tagger, templates, deterministic = True) 
      
    return trainer.train(train_sents, **kwargs)

brill_tag = train_brill_tagger(perceptron_tagger, train_sents) 
print('training finished')
for rule in brill_tag.rules():
    print(rule)
print(brill_tag.train_stats())

b = brill_tag.evaluate(test_sents)
print ("Accuracy of brill_tag : ", b) 


