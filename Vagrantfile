# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|

  config.vm.box     = 'skimurdev/skimurdev-1.0.0'
  config.vm.box_url = 'https://github.com/skimur/skimur-vagrant/releases/download/1.0.0/skimurdev_1.0.0.box'

  config.vm.define "dev" do |dev|

    dev.vm.hostname = "skimurdev"

    dev.vm.network :private_network, ip: "192.168.10.200"
    dev.vm.network :forwarded_port, guest: 5656, host: 5656, id: "skimurpostgres"
    dev.vm.network :forwarded_port, guest: 5672, host: 5672, id: "skimurrabbitmq"
    dev.vm.network :forwarded_port, guest: 15672, host: 15672, id: "skimurrabbitmqadmin"
    dev.vm.network :forwarded_port, guest: 6379, host: 6379, id: "skimurredis"
    dev.vm.network :forwarded_port, guest: 9042, host: 9042, id: "skimurcassandra"

  end

  config.vm.provider "virtualbox" do |v|
    v.memory = 4096
    v.cpus = 2
  end

  config.ssh.forward_agent = true

end
