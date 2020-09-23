//
// Created by kellm on 2020-09-22.
//

#include "Game.h"

void Game::render() {
    window->clear();
    backgroundTexture.loadFromImage(image);
    background.setTexture(backgroundTexture);
    window->draw(background);
    window->display();
}

void Game::update() {
    while (window->pollEvent(ev))
    {
        switch (ev.type) {
            case sf::Event::Closed:
                window->close();
                std::cout<<"Average Time: "<<average_time*1000<<"ms"<<std::endl;
                break;
        }
    }
    if (sf::Mouse::isButtonPressed(sf::Mouse::Button::Left)){
        auto pos = sf::Mouse::getPosition(*window);
        if (pos.x>0 && pos.y>0 && pos.x<videoMode.width && pos.y<videoMode.height){
            sand->addElement(Materials::sand, pos.x, pos.y);
        }
    }
    auto start = std::chrono::high_resolution_clock::now();
    sand->update();
    auto finish = std::chrono::high_resolution_clock::now();
    std::chrono::duration<double> delta = finish-start;
    if (average_time==0){
        average_time = delta.count();
    } else{
        average_time -= average_time/N;
        average_time += delta.count()/N;
    }


    image.create(videoMode.width, videoMode.height, sand->getPixelData());
}

Game::Game() {
    init_window();
}

void Game::init_window() {
    window = new sf::RenderWindow(videoMode,
                                  "Window", sf::Style::Titlebar | sf::Style::Close);
    image.create(videoMode.width, videoMode.height, sf::Color::Black);
    sand = new FallingSand(videoMode.width, videoMode.height);
}

Game::~Game() {
    delete window;
}

bool Game::running() const{
    return window->isOpen();
}

void Game::update_image(const uint8_t* ptr) {
    image.create(videoMode.width, videoMode.height, ptr);
}

sf::VideoMode Game::getVideoMode() const {
    return videoMode;
}
